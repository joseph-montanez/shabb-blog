namespace Contact

open System
open System.Net
open System.Net.Mail
open System.Net.Mime
open System.Threading
open System.ComponentModel
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

module Contact =
    [<DataContract>]
    type ContactData = {
        [<field: DataMember(Name = "fullname")>]
        fullname : string
        [<field: DataMember(Name = "email")>]
        email : string
        [<field: DataMember(Name = "message")>]
        message : string
    }
    type internal AsyncResultNoResult(callback: AsyncCallback, state: obj) = 
        let statePending = 0
        let stateCompletedSync = 1
        let stateCompletedAsync = 2
        let mutable completedState = statePending
        let mutable waitHandle: ManualResetEvent = null
        let mutable resultException: exn = null
        interface IAsyncResult with
            member x.AsyncState = state
            member x.AsyncWaitHandle = 
                if waitHandle = null then
                    let isDone = (x :> IAsyncResult).IsCompleted
                    let mre = new ManualResetEvent(isDone)
                    if Interlocked.CompareExchange(&waitHandle, mre, null) <> null
                        then mre.Close()
                        else
                            if not isDone && (x :> IAsyncResult).IsCompleted
                                then waitHandle.Set() |> ignore
                upcast waitHandle
            member x.CompletedSynchronously = 
                Thread.VolatileRead(&completedState) = stateCompletedSync
            member x.IsCompleted = 
                Thread.VolatileRead(&completedState) <> statePending
        member x.SetAsCompleted(e: exn, completedSynchronously: bool) = 
            resultException <- e
            let prevState = Interlocked.Exchange(&completedState, if completedSynchronously then stateCompletedSync else stateCompletedAsync)
            if prevState <> statePending
                then raise <| InvalidOperationException("You can set a result only once")
            if waitHandle <> null
                then waitHandle.Set() |> ignore
            if callback <> null
                then callback.Invoke(x)
        member x.EndInvoke() = 
            let this = x :> IAsyncResult
            if not this.IsCompleted then
                this.AsyncWaitHandle.WaitOne() |> ignore
                this.AsyncWaitHandle.Close()
                waitHandle <- null
            if resultException <> null
                then raise resultException
    type SmtpClient with
        member private x.GetAsyncResult(callback, state) : IAsyncResult = 
            let r = AsyncResultNoResult(callback, state)
            x.SendCompleted
            |> Event.add(fun args -> r.SetAsCompleted(args.Error, false))
            upcast r
        
        member x.BeginSend(message: MailMessage, callback, state) = 
            let r = x.GetAsyncResult(callback, state)
            x.SendAsync(message, null)
            r

        member x.BeginSend(from, recipients, subject, body, callback, state) = 
            let r = x.GetAsyncResult(callback, state)
            x.SendAsync(from, recipients, subject, body, null)
            r

        member x.EndSend(result: IAsyncResult) = 
            let result = result :?> AsyncResultNoResult
            result.EndInvoke()

        member x.AsyncSend(message: MailMessage) = 
            Async.FromBeginEnd((fun(iar,state) -> x.BeginSend(message, iar, state)), x.EndSend, x.SendAsyncCancel)

        member x.AsyncSend(from, recipients, subject, body) : Async<unit> = 
            Async.FromBeginEnd((fun(iar,state) -> x.BeginSend(from, recipients, subject, body, iar, state)), x.EndSend, x.SendAsyncCancel)
    //let SendEmail data:ContactData = async
        //let toAddress = Environment.GetEnvironmentVariable "PATH"