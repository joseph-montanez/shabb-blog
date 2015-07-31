[![Issue Stats](http://issuestats.com/github/joseph-montanez/ShabbBlog/badge/issue)](http://issuestats.com/github/fsprojects/ProjectScaffold)
[![Issue Stats](http://issuestats.com/github/joseph-montanez/ShabbBlog/badge/pr)](http://issuestats.com/github/joseph-montanez/ShabbBlog)

# ShabbBlog

This is a Wordpress XML parser that serves blog entries, pagination, tags and categories. Its built using [F#](http://fsharp.org/ "FSharp"), [Suave](http://suave.io/ "Suave") and [Paket](http://fsprojects.github.io/Paket/ "Paket"). This currently serves as my API for website [shabb.com](http://shabb.com "Shabb")

In order to build the application run

    $ build.cmd // on windows    
    $ build.sh  // on mono
    
In order to run the application run

    $ src\ShabbBlog\bin\Release\ShabbBlog.exe

This runs on port 8083, you can access the blog API via http://localhost:8083/

## Maintainer(s)

- [@joseph-montanez](https://github.com/joseph-montanez)
