# Myucel.NET

Myucel.NET is a small library that search Reddit's /r/anime discussion threads using Reddit's search API.

It's something I made for myself to easily find anime discussion threads. I'm sure there are beter ways to do this. Currently even a slight typo can throw off Reddit search algorithm.

Myucel.NET is a .NET Standard Library targeting .NET Standard 1.1 (Refer to compatibility table [here](https://blogs.msdn.microsoft.com/dotnet/2016/09/26/introducing-net-standard/))

### Usage

Create Myucel object then use the FindSubmission method.

```c#
var myucel = new Myucel();
var submission = myucel.FindSubmission("Outbreak Company", 1);
var url = submission.First().Link;
```
