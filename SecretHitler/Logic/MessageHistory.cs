using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Logic
{
    class MessageHistory : IEnumerable<string>
    {
        public static MessageHistory Instance { get; } = new MessageHistory();
        public event Action<string> OnNewMessage;
        private LinkedList<string> msgs = new LinkedList<string>();
        private MessageHistory()
        {

        }
        public void AddHistory(string txt)
        {
            msgs.AddLast(txt);
            OnNewMessage?.Invoke(txt);
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var str in msgs)
                yield return str;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
}
