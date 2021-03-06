using System;
using System.Data;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;

namespace Marten.Schema.Sequences
{
    public class HiLoSequence : ISequence
    {
        private readonly string _entityName;
        private readonly object _lock = new object();
        private readonly ICommandRunner _runner;

        public HiLoSequence(ICommandRunner runner, string entityName, HiloDef def)
        {
            _runner = runner;
            _entityName = entityName;

            CurrentHi = -1;
            CurrentLo = 1;
            MaxLo = def.MaxLo;
            Increment = def.Increment;
        }

        public string EntityName => _entityName;

        public long CurrentHi { get; private set; }
        public int CurrentLo { get; private set; }

        public int MaxLo { get; }
        public int Increment { get; private set; }

        public int NextInt()
        {
            return (int) NextLong();
        }

        public long NextLong()
        {
            lock (_lock)
            {
                if (ShouldAdvanceHi())
                {
                    AdvanceToNextHi();
                }

                return AdvanceValue();
            }
        }

        public void AdvanceToNextHi()
        {
            _runner.Execute(conn =>
            {
                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    var raw = conn.CreateSprocCommand("mt_get_next_hi")
                        .With("entity", _entityName)
                        .Returns("next", NpgsqlDbType.Bigint).ExecuteScalar();

                    CurrentHi = Convert.ToInt64(raw);

                    tx.Commit();
                }
            });

            CurrentLo = 1;
        }

        public long AdvanceValue()
        {
            var result = (CurrentHi*MaxLo) + CurrentLo;
            CurrentLo++;

            return result;
        }


        public bool ShouldAdvanceHi()
        {
            return CurrentHi < 0 || CurrentLo > MaxLo;
        }
    }
}