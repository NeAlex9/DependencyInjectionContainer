using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionContainer
{
    public interface IRep { }

    public class Rep : IRep { }

    public interface IInterface<TRep> where TRep : IRep
    {
        TRep Fuck { get; }
    }

    public class Ex<TRep> : IInterface<TRep>
        where TRep : IRep
    {
        public Ex(TRep TImpl)
        {
            this.Fuck = TImpl;
        }

        public TRep Fuck { get; }
    }

    public interface IMessageSender
    {
        string SendMessage();
    }

    public class Email : IMessageSender
    {
        public IRep Rep;
        public Email(IRep rep)
        {
            this.Rep = rep;
        }

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
