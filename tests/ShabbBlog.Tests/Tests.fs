module ShabbBlog.Tests

open ShabbBlog
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = Blog.hello 42
  printfn "%i" result
  Assert.AreEqual(42,result)
