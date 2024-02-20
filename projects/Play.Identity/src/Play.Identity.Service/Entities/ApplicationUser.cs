using System;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace Play.Identity.Service.Entities;

[CollectionName("Users")]
public class ApplicationUser : MongoIdentityUser<Guid>
{
    /// <summary>
    /// THe amount of the User Gil
    /// </summary>
    public decimal Gil { get; set; }
}
