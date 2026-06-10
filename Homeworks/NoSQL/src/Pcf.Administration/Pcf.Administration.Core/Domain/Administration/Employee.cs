using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Pcf.Administration.Core.Domain.Administration
{
    public class Employee
        : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; }

        public int AppliedPromocodesCount { get; set; }
    }
}