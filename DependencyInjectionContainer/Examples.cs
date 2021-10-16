using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionContainer
{
    public interface IRep { }

    public interface IInterface<TRep> where TRep : IRep { }

    public class Ex<TRep> : IInterface<TRep>
        where TRep : IRep
    {
        public IInterface<TRep> Field;
        public Ex(IInterface<TRep> TImpl)
        {
            this.Field = TImpl;
        }
    }

    public interface IMessageSender
    {
        string SendMessage();
    }

    public class Email : IMessageSender
    {
        public string SendMessage()
        {
            throw new NotImplementedException();
        }
    }

    public class Letter : IMessageSender
    {
        public string SendMessage()
        {
            throw new NotImplementedException();
        }
    }
}
