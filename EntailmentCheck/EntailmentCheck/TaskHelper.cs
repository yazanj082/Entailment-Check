using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntailmentCheck
{
    internal class TaskHelper
    {
        IConfiguration _configuration;
        List<string> conditions;
        Dictionary<string,string> names;
        List<char> chars;
        string[] conditionWords;
        List<string> conditionWordsSymbols;
        List<string> conditionWordsSymbols1;
        Dictionary<string, string> specialSympols;
        Dictionary<string, string> toReplace;
        Dictionary<string, List<bool>> data;
        public TaskHelper(IConfiguration configuration)
        {
            this._configuration = configuration;
            conditions = new List<string>();
            names = new Dictionary<string, string>();
            data = new Dictionary<string, List<bool>>();
            toReplace = new Dictionary<string, string>();
            chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().ToList();
            conditionWords = new string[] { " and ", " then ", " or ", "not ", "if " };
            conditionWordsSymbols = new List<string> { " ٨ ", " ٧ " , "¬" ," => ", " <=> " };
            conditionWordsSymbols1 = new List<string> { "٨", "٧","=>","<=>" };
            specialSympols = new Dictionary<string, string>() { { "if ","" }, { "not ", "¬" }, { "and", "٨" }, { "then","->"},{ "or", "٧" } };
        }
        public void Execute()
        {
            var path = _configuration["PathToFile"];
            BuildKnowledgeBase(path);
            PrintTable();
            var condition = NewCondition();
            Console.WriteLine(CheckNewCondition(condition));
        }
        string CheckNewCondition(string str)
        {
            string result = str + " entiles the KB";
            List<List<bool>> logics = new List<List<bool>>();
            var temp = str.Split(conditionWordsSymbols.ToArray(), StringSplitOptions.None).ToList();
            temp.Remove("");
            foreach (string word in temp)
            {
                if (!names.ContainsKey(word))
                {
                    data[word] = new List<bool>();
                }
            }
            var count = Math.Pow(2, data.Count)/2;
            bool positive;
            foreach (var list in data.Values)
            {
                positive = false;
                for (int i = 1; i <= Math.Pow(2, data.Count); i++)
                {
                    if(positive)
                        list.Add(true);
                    else
                        list.Add(false);
                    if(i%count == 0)
                        positive = !positive;

                }
                count/=2;
            }
            var toCheck = ToLogic(str);
            foreach(var condition in conditions)
            {
                logics.Add(ToLogic(condition));
            }
            for(int i = 0; i < logics[0].Count; i++)
            {
                bool cont = true;
                foreach (var list in logics)
                {
                    if (list[i] == false)
                    {
                        cont = false;
                    }
                }
                if (cont)
                {
                    if (!toCheck[i])
                    {
                        result = str + " does not entile the KB";
                        break;
                    }
                }
            }
            return result;

        }
        List<bool> ToLogic(string str)
        {
            foreach(var s in toReplace)
            {
                str=str.Replace( s.Key, s.Value);
            }
            string temp= str;
            List<bool> first=null;
            if (str.Contains("("))
            {
                temp = temp.Substring(temp.IndexOf('(')+1, temp.IndexOf(')')- temp.IndexOf('(')-1);
                str=str.Replace(temp, "");
                str = str.Replace("(", "");
                str = str.Replace(")", "");
                first = ToLogic(temp);
            }
            var dd = str.Split(" ");
            if (first != null)
            {
                var d = dd.ToList();
                d.Remove("");
                return operate(d.ToArray(), first);
            }
            else
            {
                return operate(dd);
            }

        }
        List<bool> operate(string[] str, List<bool> first = null)
        {
            var op = "";
            List<bool> second = null;
            List<bool> holder;
            List<bool> result = new List<bool>();
            for (int i=0; i < str.Length; i++)
            {
                    if(i == 0 && str[0]== "¬" && first != null)
                {
                    for (int j = 0; j < first.Count; j++)
                    {
                        first[j]=!first[j];
                    }
                    continue;
                }
                    result = new List<bool>();
                    if (str[i].ElementAt(0) == '¬')
                    {
                        var dd = data[(str[i].ElementAt(1)).ToString()];
                        holder = new List<bool>();
                        for(int j=0;j<dd.Count;j++)
                        {
                            holder.Add(!dd[j]);
                        }
                    }
                    else if(str[i].Count() > 1)
                    {
                        op = str[i];
                        continue;
                    }
                    else if (conditionWordsSymbols1.Contains(str[i]))
                    {
                        op = str[i];
                        continue;
                    }
                    else
                    {
                        holder = data[str[i]];
                    }
                    if (first == null)
                        first = holder;
                    else if(second == null)
                        second = holder;
                    else
                    {
                        if(op == "٨")
                        {
                            for(int j=0;j< first.Count; j++)
                            {
                                result.Add(first[j]&second[j]);
                            }
                        }
                        if (op == "٧")
                        {
                            for (int j = 0; j < first.Count; j++)
                            {
                                result.Add(first[j] | second[j]);
                            }
                        }
                        if (op == "=>")
                        {
                            for (int j = 0; j < first.Count; j++)
                            {
                                result.Add(Implies(first[j] , second[j]));
                            }
                        }
                        if (op == "<=>")
                        {
                            for (int j = 0; j < first.Count; j++)
                            {
                                result.Add(Xnor(first[j] , second[j]));
                            }
                        }
                    first = result;
                        second = null;
                        op = "";
                    }
                

            }
            
            if (op == "٨")
            {
                for (int j = 0; j < first.Count; j++)
                {
                    result.Add(first[j] & second[j]);
                }
            }
            if (op == "٧")
            {
                for (int j = 0; j < first.Count; j++)
                {
                    result.Add(first[j] | second[j]);
                }
            }
            if (op == "=>")
            {
                for (int j = 0; j < first.Count; j++)
                {
                    result.Add(Implies(first[j], second[j]));
                }
            }
            if (op == "<=>")
            {
                for (int j = 0; j < first.Count; j++)
                {
                    result.Add(Xnor(first[j], second[j]));
                }
            }
            return result;
        }
        bool Xnor(bool first, bool second)
        {
            if (first == second)
                return true;
            return false;
        }
        bool Implies(bool first, bool second)
        {
            if (first == false && second == true)
                return false;
            return true;
        }
        string NewCondition()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("please enter the new condition");
            var input = Console.ReadLine();
            input = input.Replace("&", "٨");
            input = input.Replace("|", "٧");
            input = input.Replace("!", "¬");
            return input;
        }
        void PrintTable()
        {
            foreach(var name in names)
            {
                Console.WriteLine("  "+name.Key + "  |  "+name.Value);
            }
            Console.WriteLine();
            Console.WriteLine("Rules: 1-the condition should contain only one group () and should be the at the first term");
            Console.WriteLine("Rules: 2-you should use & instead of ٨ , | instead of ٧ and ! instead of ¬");
            Console.WriteLine("Rules: 3-you should put spaces between terms EX:");
            Console.WriteLine("(!a | b) & b");
        }
        void BuildKnowledgeBase(string path)
        {
            var lines = File.ReadAllLines(path);
            List<string> temp;
            string str;
            foreach (var line in lines)
            {
                temp = line.Split(conditionWords, StringSplitOptions.None).ToList();
                str = line;
                foreach (string word in temp)
                {
                    if(word == string.Empty)
                        continue;
                    if (word.Contains("not"))
                    {

                    }
                    if (!names.ContainsValue(word))
                    {
                        names.Add(chars[0].ToString(), word);
                        data[chars[0].ToString()] = new List<bool>();
                        chars.RemoveAt(0);
                    }
                }
                foreach(var name in names)
                {
                    str = str.Replace(name.Value, (name.Key).ToString());
                }
                str = str.Replace(".", "");
                str = str.Replace(",", "");
                //str = str.Replace(" ", "");
                foreach(var symbol in specialSympols)
                {
                    str = str.Replace(symbol.Key, symbol.Value);
                }
                if (str.Contains("->"))
                {
                    toReplace[str.Split(" -> ")[1]] = "("+str.Split(" -> ")[0]+")";
                    data.Remove(str.Split(" -> ")[1]);
                    str = str.Split(" ->")[0];
                }
                //if (str.Contains("not "))
                //    str = str.Replace("not ", ("!"));
                conditions.Add(str);

            }
        }
    }
}
