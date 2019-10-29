Please follow these instructions to build the web crawler program.

Assuming you are on a Windows machine that has .NET installed

Step 1
Navigate to the directory where you have unzipped Program.cs, myWebCrawler.csproj, and webBie_Builder.bat
in the cmd terminal.

Step 2
Type 'webBie_Builder.bat'

Step 3
Type 'myWebCrawler.exe (starting URL) (number of Hops to make from starting URL)' and hit enter.
**********************************************************************************************

If you are on Linux, please make sure that wine is installed so that you can run .bat files
and .NET to run the C# program

In the terminal....
Type 'wine cmd' to run the Windows-Console in the Linux terminal. Then follow the same steps as 
for Windows

If in native Linux shell, you can try

Step 1
Navigate to the directory wher you have unzipped Program.cs, myWebCrawler.csproj, and webBie_Builder.bat

Step 2
Type 'wine cmd.exe /c webBie_Builder.bat'

Step 3
Type 'myWebCrawler.exe (starting URL) (number of Hops to make from starting URL)' and hit enter. 
**********************************************************************************************
myWebCrawler Description and Assumptions:

The web crawler will look for the first unvisited http or https <a> tag hrefs in the HTML of the passed in
absolute URL and 'hop' to that next link. This will be repeated for the number of 'hop' times that
is passed in when initially running the program.

myWebCrawler will continue to hop as long as the response code from the absolute URL is 2xx and print
the ending URL and the HTML content of that page.

myWebCrawler will end on any page with a 4xx or 5xx response code and print that page's URL and HTML.

If my myWebCrawler runs into a 3xx response code, it will attempt to retrieve the redirected location
and retry the hop to that location.