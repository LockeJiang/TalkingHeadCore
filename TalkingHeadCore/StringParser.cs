using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkingHeadCore
{
    class StringParser
    {
        Queue<QueueElement> emotions = new Queue<QueueElement>();
        string result;
        public StringParser(String input)
        { 
            char[] separator = {'{', '}'};
            string[] splitted = input.Split(separator);
            result = "";
            int i=isEmotion(splitted[0])?0:1;
            int currentPosition=0;
            for (; i <=splitted.Length; i+=2)
            {
                if (i - 1 >= 0)
                {
                    currentPosition += splitted[i - 1].Length;
                    result += splitted[i - 1];
                }
                if (i!=splitted.Length) emotions.Enqueue(new QueueElement(splitted[i], currentPosition));
            }

        }

        public bool isEmotion(String input)
        {
            switch (input)
            {

                case "normal":
                    return true;
                case "sad":
                    return true;
                case "happy":
                    return true;
                case "angry":
                    return true;
                case "fear":
                    return true;
                case "surprise":
                    return true;
                case "interest":
                    return true;
                case "revulsion":
                    return true;
                case "shame":
                    return true;
            }
            return false;
        }

        public string Result
        {
            get { return result; }
            set { result = value; }
        }

        public string getEmotion(int position)
        {
            if (this.emotions.Count == 0) return null;
            if (this.emotions.Peek().Position <= position)
                return this.emotions.Dequeue().Emotion;
            else
                return null;
        }

        public Boolean isHaveRussianCharacters()
        {
            foreach(char c in result)
            {
                if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я'))
                    return true;
            }
            return false;
        }
    }
}
