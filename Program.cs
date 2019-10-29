using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // handle invalid input of args
        if (args.Length <= 1)
        {
            invalidArgs();
        }
        else
        {
            runCrawler(args[0], args[1]);
        }
    }

    public static void runCrawler(string baseUrl, string hops)
    {
        string[] urlAccept = { "http://", "https://" };
        // check if curUrl is given correctly
        if (!checkUrl(baseUrl, urlAccept) || baseUrl.Length < 7)
        {
            Console.WriteLine("Bad / non (http:// or https://) URL given"
                            + "\nPlease try again");
        }
        else
        {
            var curUrl = baseUrl;
            int max_NumHops = 0;
            try
            {
                max_NumHops = Convert.ToInt32(hops);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Not a valid int string passed in.\nWill Hop 0 times."
                                + "\nTry again with correct arguments.");
            }
            int numHops = 0;
            char[] trimEnd = { '/', '>' };
            char[] trimStart = { 't', 'p', 's' };
            string result;  // store URL content
            string _base = "ht"; // base of all absolute URL

            // pattern to look for only <a> tags with href="http(s):// links                     
            string pattern = "<a(\\s.*?)href\\s*=\\s*(?:[\"']ht(?<1>[^\"']*)[\"']|ht(?<1>\\S+))";
            Console.WriteLine("=======================================================");
            Console.WriteLine("Starting webBie at url = \n" + curUrl + "\nWebBie will attempt to hop -"
                                + max_NumHops + "- times");
            Console.WriteLine("=======================================================");
            // create set to store previously visited URL
            HashSet<string> visited = new HashSet<string>();
            // add the starting URL to visited set. remove trailing "/"
            visited.Add(curUrl.TrimEnd('/'));
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                for (; numHops < max_NumHops; numHops++)
                {
                    // get the resoponse message for the current URL
                    HttpResponseMessage response = client.GetAsync(curUrl).Result;
                    // get status code
                    int status = (int)response.StatusCode;
                    if (numHops > 0)
                    {
                        Console.WriteLine("=======================================================");
                        Console.WriteLine("Hop number -" + numHops + "- to " + curUrl);
                        Console.WriteLine("=======================================================");
                    }
                    // handle the returned code
                    if (response.IsSuccessStatusCode)
                    {
                        // grab the body content
                        result = response.Content.ReadAsStringAsync().Result;
                        Match m;  // will be used to find matches in the HTML string result
                        try
                        {
                            m = Regex.Match(result, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            if (m.Success)
                            {
                                // check if we have previously visited this site at any point
                                while (!visited.Add(m.Groups[1].ToString().TrimStart(trimStart).TrimEnd(trimEnd)))
                                {
                                    // find next match to check
                                    m = m.NextMatch();
                                }
                                // if all links on current page have been visited
                                if (!m.Success)
                                {
                                    Console.WriteLine("No more unvisited absolute URLs on this page");
                                    break;
                                }
                                // hop to next link 
                                curUrl = _base + m.Groups[1].ToString().TrimEnd(trimEnd);
                            }
                            // no <a> tag matched
                            else
                            {
                                Console.WriteLine("No match for pattern with <a> tag and "
                                + "href=(\"http:// or \"https://)");
                                // finish hopping
                                break;
                            }
                        }
                        catch (RegexMatchTimeoutException e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("Ran into a RegexMatchTimeoutException");
                            Console.WriteLine("**Please try again**");
                        }
                    }
                    else if (300 <= status && status <= 399)
                    {
                        // URL moved
                        Console.WriteLine("Hop number -" + numHops + "- for " + curUrl + " = " + status
                                            + "\nwas redirected");
                        // find new url from header and hop there
                        string header = response.Headers.ToString();
                        string lPattern = "Location:(\\s.*?)(?:ht(?<1>\\S+))";
                        // 
                        Match p = Regex.Match(header, lPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        if (p.Success)
                        {
                            Console.WriteLine("Location = " + _base + p.Groups[1].ToString());
                        }
                        else
                        {
                            Console.WriteLine("No Redirected Location found in response Header from:\n"
                                            + curUrl);
                            break;
                        }
                        // retry this hop
                        numHops--;
                        Console.WriteLine("Retrying the hop to redirected url");
                        curUrl = _base + p.Groups[1].ToString();
                    }
                    else if (400 <= status && status <= 499)
                    {
                        // URL not found
                        urlNotFound(response.StatusCode, status);
                        break;
                    }
                    else if (500 <= status && status <= 599)
                    {
                        // Server Down
                        serverDown(response.StatusCode, status);
                        break;
                    }
                }
                // Final response message, we have arrived at the final hop
                HttpResponseMessage fresponse = client.GetAsync(curUrl).Result;
                // Deal with response message
                result = fresponse.Content.ReadAsStringAsync().Result;
                Console.WriteLine("=======================================================");
                Console.WriteLine("Total number of Hops = " + numHops);
                Console.WriteLine("Ended on URL: " + curUrl + " status code = " + (int)fresponse.StatusCode);
                Console.WriteLine("=======================================================");
                Console.WriteLine("**HTML Content:");
                Console.WriteLine(result);
            }
        }
    }

    /********** checkUrl(string, string[]) **********
     * This method will check to see if the passed in string url starts with valid
     * absolute url characters. Matching any of the passed in values in the accepted
     * string array.
     * Returns = true if url matches any of the values in accepted[], otherwise
     *              returns false.
     */
    public static Boolean checkUrl(string url, string[] accepted)
    {
        for (int i = 0; i < accepted.Length; i++)
        {
            if (url.StartsWith(accepted[i]))
                return true;
        }
        return false;
    }

    /********** serverDown(HttpStatusCode, int) **********
     * This method is used to handle a 5xx HttpResponseMessage.
     */
    public static void serverDown(HttpStatusCode code, int status)
    {
        Console.WriteLine("Reached status code: " + status);
        Console.WriteLine("This action is: " + code + "\nPlease try again");
    }

    /********** urlNotFound(HttpStatusCode, int) **********
    * This method is used to handle a 4xx HttpResponseMessage
    */
    public static void urlNotFound(HttpStatusCode code, int status)
    {
        Console.WriteLine("Reached status code: " + status);
        Console.WriteLine("This action is: " + code + "\nPlease try again");
    }

    /********** invalidArgs() **********
     * This method is used to handle invalid args passed in when starting
     * the program.
     */
    public static void invalidArgs()
    {
        Console.WriteLine("=======================================================");
        Console.WriteLine("Not enough arguments were used when running the program\n"
                        + "Please pass in a starting http:// URL and the number of\n"
                        + "hops you would like to make from that URL.\nThank you");
        Console.WriteLine("=======================================================");
    }
}


