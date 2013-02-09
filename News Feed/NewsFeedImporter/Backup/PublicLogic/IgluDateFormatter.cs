using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PublicLogic
{
    public class IgluDateFormatter : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// Private class that handles the Case Operation for any part of the DateTime String
        /// </summary>
        private enum CaseOperation
        {
            Upper,
            Lower,
            None
        }

        #region ICustomFormatter Members

        /// <summary>
        /// What characters do we support
        /// d = Day
        /// M = Month
        /// y = Year
        /// </summary>
        private string[] _characters = { "d", "M", "y" };

        /// <summary>
        /// Format the string between indexes
        /// </summary>
        /// <param name="format">The format of the string, i.e. ddMMyyyy</param>
        /// <param name="charStartIndex">Where to start formatting, i.e. Index 5</param>
        /// <param name="charEndIndex">Where to end formatting, i.e. Index 6</param>
        /// <returns>the Format Parameter, formatted between charStartIndex and charEndIndex</returns>
        private static string FormatString(string format, int charStartIndex, int charEndIndex, DateTime dateTimeToUse)
        {
            //This is where we store the formatted string, for insersion into the final string
            string tempResult = "";
            CaseOperation caseOperation = CaseOperation.None;

            //This is where we store the format of the datetime we want to get out
            string stringToFormat = format.Substring(charStartIndex, (charEndIndex - charStartIndex)).Trim();

            //Is this character the start of our Addition of Time?!
            if (format[charEndIndex] == '[')
            {
                int additionStartIndex = -1;
                int additionEndIndex = -1;

                //Loop through our array until we find the ending ], which closes the Addition block ([+1])
                for (int a = charEndIndex; a < format.Length; a++)
                {
                    if (additionStartIndex == -1)
                        additionStartIndex = a;

                    //Check to see if we hit the end of the addition block
                    if (format[a] == ']')
                    {
                        additionEndIndex = a;
                        break;
                    }
                }

                //TODO: Make Regex
                string additionResult = Regex.Replace(format.Substring(additionStartIndex, (additionEndIndex - additionStartIndex)), @"(\[)|(\+)", "");

                //Check our Addition for a #. If we have a #, then we need to check to see what it's value is. It is what allows us to return a Date String in Lower/Upper Case
                if (additionResult.Contains('#'))
                {
                    //We have the #! Let's remove it and get it's value
                    int caseFormatterIndex = additionResult.IndexOf('#');

                    //Check to see if there is the following character
                    if (additionResult.Length > (caseFormatterIndex + 1) && Char.IsLetter(additionResult[caseFormatterIndex + 1]))
                    {
                        //There is, store it for later
                        switch (additionResult[caseFormatterIndex + 1])
                        {
                            case 'U':
                                caseOperation = CaseOperation.Upper;
                                break;
                            case 'L':
                                caseOperation = CaseOperation.Lower;
                                break;
                            default:
                                caseOperation = CaseOperation.None;
                                break;
                        }

                        //Remove the #U/#L
                        additionResult = additionResult.Remove(caseFormatterIndex, 2);
                    }
                    else
                    {
                        //It's there, just not with an operator...so just remove the #
                        additionResult = additionResult.Remove(caseFormatterIndex, 1);
                    }
                }

                //Lets make sure it tries to add something, just so it doesn't crash. We'll add 0.
                if (string.IsNullOrEmpty(additionResult))
                    additionResult = "0";

                //Decide what we need to add
                switch (stringToFormat[0])
                {
                    case 'y':
                        //It's a year field, add years and return the formatted string
                        tempResult = dateTimeToUse.AddYears(Convert.ToInt32(additionResult)).ToString(stringToFormat);
                        break;
                    case 'M':
                        //It's a month field, add month and return the formatted string
                        tempResult = dateTimeToUse.AddMonths(Convert.ToInt32(additionResult)).ToString(stringToFormat);
                        break;
                    case 'd':
                        //It's a day field, add day and return the formatted string
                        tempResult = dateTimeToUse.AddDays(Convert.ToInt32(additionResult)).ToString(stringToFormat);
                        break;
                    default:
                        break;
                }

                //Deal with the Case
                switch (caseOperation)
                {
                    case CaseOperation.Upper:
                        tempResult = tempResult.ToUpper();
                        break;
                    case CaseOperation.Lower:
                        tempResult = tempResult.ToLower();
                        break;
                    case CaseOperation.None:
                        break;
                }
            }
            else
            {
                //No, it's just a standard time...
                tempResult = dateTimeToUse.ToString(stringToFormat);
            }

            return tempResult;
        }

        /// <summary>
        /// This is the method that .NET Calls, when we pass in a IgluDateFormatter to string.format
        /// </summary>
        /// <param name="format">The format of the string</param>
        /// <param name="arg">The object of which we should be formatting</param>
        /// <param name="formatProvider">The format provider</param>
        /// <returns>A fully formatted string</returns>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is DateTime))
                throw new ArgumentException("IgluDateFormatter only accepts System.DateTime");

            DateTime dateTimeToUse = (DateTime)arg;

            if (format.Contains('['))
            {
                //These are used to track sections, so we know where it starts and ends, i.e. 'ddddMMyyyy' the MM Section begins at index 5, and ends at index 6
                int charStartIndex = -1;
                int endIndex = -1;

                //Loop until we find a supported character
                for (int i = 0; i < format.Length; i++)
                {
                    bool shouldFormat = false;

                    if (_characters.Contains<string>(format[i].ToString()))
                    {
                        //If we don't have a start index, this is what it will become
                        if (charStartIndex == -1)
                            charStartIndex = i;
                        //If we DO have a start index, we might need to begin processing
                        else
                        { 
                            //Check to see if this character is the same the last one...
                            if (format[i] != format[i - 1] || i == (format.Length - 1))
                            {
                                //We have found the end of this block, i.e. 'dd'
                                endIndex = i;

                                //It should now format, we have reached the end of this block
                                shouldFormat = true;
                            }
                        }
                    }
                    else
                    {
                        //If we have a start index, set the end index and prepare to format
                        if (charStartIndex != -1)
                        {
                            //We have found the end of this block, i.e. 'dd'
                            endIndex = i;

                            //It should now format, we have reached the end of this block
                            shouldFormat = true;
                        }
                    }

                    //Should we begin formatting the string?
                    if (shouldFormat)
                    {
                        //Format our string, with the correct info
                        string resultantValue = FormatString(format, charStartIndex, endIndex, dateTimeToUse);

                        //Remove the original string, i.e. dddd
                        format = format.Remove(charStartIndex, ((i == (format.Length - 1) ? endIndex + 1 : endIndex) - charStartIndex));

                        //Insert the new string (Saturday) in place of the old (dddd)
                        format = format.Insert(charStartIndex, resultantValue);

                        //We need to go to the end of the item we just added, for e.g. we had dddd in the field. that returns as saturday, so as soon as it hits the d, it'll try to format that.
                        i += ((resultantValue.Length - (endIndex - charStartIndex)) - 1);

                        //We are done with the string, we can move onto the next block now (if there is one...) so reset values back to -1
                        charStartIndex = -1;
                        endIndex = -1;
                    }
                }

                //Remove the unwanted [...] that were left over, and return the final string
                return Regex.Replace(format, @"(\|)|(\[.+?\])", "", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            }
            else
            {
                //It's not a special string, just format as usual using .NETs method
                return dateTimeToUse.ToString(format);
            }
        }

        #endregion

        #region IFormatProvider Members

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }

        #endregion
    }
}
