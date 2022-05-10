﻿using SecureDocuments.Models.Common;

namespace SecureDocuments.Models
{
    public class CustomerEmail : ValueObject
    {
        public CustomerEmail(string value)
        {
            Value = value;
        }

        public string Value { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}