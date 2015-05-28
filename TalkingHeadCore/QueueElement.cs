using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkingHeadCore
{
    class QueueElement
    {
        string emotion;
        int position;
        public QueueElement(string _emotion, int _position)
        {
            this.emotion = _emotion;
            this.position = _position;
        }

        public QueueElement(string _emotion, int _position, QueueElement _next)
        {
            this.emotion = _emotion;
            this.position = _position;
        }


        public string Emotion
        {
            set { emotion = value; }
            get { return emotion;}
        }

        public int Position
        {
            set { position = value; }
            get { return position;  }
        }
    }
}
