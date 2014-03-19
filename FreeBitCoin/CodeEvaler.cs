using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace FreeBitCoin
{
    public class CodeEvaler
    {
        public void Run()
        {
            MakeRequests();
        }

        //Calls request functions sequentially.
        private void MakeRequests()
        {
            HttpWebResponse response;
            string responseText;

            if (Request_freebitco_in(out response))
            {
                //Success, possibly use response.
                responseText = ReadResponse(response);
                CleverBet.Instance.SetMoneys(responseText);

                response.Close();
            }
            else
            {
                //Failure, cannot use response.
            }
        }

        //Returns the text contained in the response.  For example, the page HTML.  Only handles the most common HTTP encodings.
        private string ReadResponse(HttpWebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                Stream streamToRead = responseStream;
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                }

                using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Tries to request the URL: http://freebitco.in/?op=double_your_btc&amp;m=hi&amp;client_seed=U8wu43HhWNRcXt2P&amp;jackpot=0&amp;stake=0.00000001&amp;multiplier=8
        /// </summary>
        /// <param name="response">After the function has finished, will possibly contain the response to the request.</param>
        /// <returns>True if the request was successful; false otherwise.</returns>
        private bool Request_freebitco_in(out HttpWebResponse response)
        {
            response = null;

            try
            {
                //Create request to URL.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://freebitco.in/?op=double_your_btc&m=" + CleverBet.Instance.GetBetHighOrLow() + "&client_seed=J3NL5rH1qiaEmIfE&jackpot=0&stake=" + CleverBet.Instance.GetStakeFormatted() + "&multiplier=" + CleverBet.Instance.GetMultiplier().ToString());

                //Set request headers.
                request.KeepAlive = true;
                request.Accept = "*/*";
                request.Headers.Add("X-Requested-With", @"XMLHttpRequest");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.154 Safari/537.36";
                request.Referer = "http://freebitco.in/";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,el;q=0.6");

                request.Headers.Set(HttpRequestHeader.Cookie, CleverBet.Instance.GetCookie());
                //request.Headers.Set(HttpRequestHeader.Cookie, @"__cfduid=**********************************************; btc_address=**********************************; password=****************************************************************; last_play=1395145853; __utma=120334047.1029067788.1394998563.1395142395.1395146077.17; __utmc=120334047; __utmz=120334047.1394998563.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none)");
                

                //Get response to request.
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                Console.WriteLine(e.ToString());

                //ProtocolError indicates a valid HTTP response, but with a non-200 status code (e.g. 304 Not Modified, 404 Not Found)
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                if (response != null) response.Close();
                return false;
            }

            return true;
        }

    }
}
