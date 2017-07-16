using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace StreamCapture
{
    public class Keywords
    {
        Dictionary<string, KeywordInfo> keywordDict;

        public Keywords(IConfiguration configuration)
        {
            try
            {
                keywordDict = JsonConvert.DeserializeObject<Dictionary<string, KeywordInfo>>(File.ReadAllText("keywords.json"));
            }
            catch(Exception e)
            {
                //Send alert mail
                string body="Keyword load failed with Exception "+e.Message;
                body=body+"\n"+e.StackTrace;
                new Mailer().SendErrorMail(configuration,"ERROR: keywords.json Exception ("+e.Message+")",body);

                throw new Exception("Problem deserializing keywords.json",e);
            }                
        }

        //Given a show name, see if there's a match in any of the keywords
        public Tuple<KeywordInfo,int> FindMatch(ScheduleShow scheduleShow)
        {
            //Go through each keyword section, seeing if there's a match for the show 
            KeyValuePair<string, KeywordInfo>[] kvpArray = keywordDict.ToArray();
            for(int kvpIdx=0;kvpIdx<kvpArray.Length;kvpIdx++)
            {
                //Loop through all keyword rows checking for a match
                bool showMatched=CheckForMatchHelper(kvpArray[kvpIdx].Value.keywords, scheduleShow.name);

                //Make sure categories match (if empty, then no-op)
                if(showMatched)
                {
                    List<string> rows=kvpArray[kvpIdx].Value.categories;
                    if(rows!=null && rows.Count>0)
                        showMatched = CheckForMatchHelper(rows, scheduleShow.category);                  
                }

                //Look for exclusions to verify match
                if(showMatched)
                {
                    bool excludeMatched = false;
                    List<string> rows=kvpArray[kvpIdx].Value.exclude;
                    if(rows!=null && rows.Count>0)                    
                        excludeMatched = CheckForMatchHelper(rows, scheduleShow.name);

                    //If we're still good, return the match
                    if(!excludeMatched)
                    {             
                        return new Tuple<KeywordInfo, int>(kvpArray[kvpIdx].Value, kvpIdx);
                    }
                }
            }

            return null;
        }

        //See if there's a match in a list of strings
        private bool CheckForMatchHelper(List<string> rows,string stringToMatch)
        {
            bool matchedFlag = true;

            //make sure there are rows
            if (rows == null || rows.Count < 1)
                return false;

            //Loop through each 'row' in the list
            foreach (string row in rows)
            {
                //we'll start with true and then get proven otherwise
                matchedFlag = true;

                //Grab the AND values and then loop through them
                string[] itemArray = row.Split(','); 
                for (int i = 0; i< itemArray.Length; i++)
                {
                    //We'll use regex to do the matching (all on lower case)
                    Regex regex = new Regex(itemArray[i].ToLower());
                    Match match = regex.Match(stringToMatch.ToLower());

                    if(!match.Success)
                    {
                        matchedFlag=false;
                        break;
                    }
                }

                //Each row is an OR, so if we're matched, exit of the loop as we're done
                if(matchedFlag)
                    break;
            }
            return matchedFlag;
        }
    }
}
 