﻿using System;
using Baseline;
using Marten.Testing.Fixtures;

namespace Marten.Testing.Documents
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool Internal { get; set; }

        public string FullName => "{0} {1}".ToFormat(FirstName, LastName);
        
        public int Age { get; set; }
    }
}