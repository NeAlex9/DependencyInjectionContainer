using System;
using System.Collections.Generic;

namespace DependencyInjectionContainer.Test
{
    public interface IRep { }

    public class Rep : IRep
    {

    }

    public interface IInterface<TRep> where TRep : IRep
    {
        TRep F { get; }
    }

    public class Ex<TRep> : IInterface<TRep>
        where TRep : IRep
    {
        public Ex(TRep TImpl)
        {
            this.F = TImpl;
        }

        public TRep F { get; }
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

    public class Chat : IMessageSender
    {
        public IEnumerable<IRep> Reps;
        public Chat(IEnumerable<IRep> reps)
        {
            this.Reps = reps;
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
