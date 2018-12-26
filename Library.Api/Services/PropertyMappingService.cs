using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Api.Entities;
using Library.Api.Models;

namespace Library.Api.Services
{
   //this is part of lecture 8 
    public class PropertyMappingService : IPropertyMappingService
   {
       //Map of properties for Searching and filtering
      private Dictionary<string, PropertyMappings> _authorPropertyMapping =
         new Dictionary<string, PropertyMappings>(StringComparer.OrdinalIgnoreCase)
         {
            { "Id", new PropertyMappings(new List<string>() { "Id" } ) },
            { "Genre", new PropertyMappings(new List<string>() { "Genre" } )},
            { "Age", new PropertyMappings(new List<string>() { "DateOfBirth" } , true) },
            { "Name", new PropertyMappings(new List<string>() { "FirstName", "LastName" }) }
         };


      private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

      public PropertyMappingService()
      {
         propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
      }
      public Dictionary<string, PropertyMappings> GetPropertyMapping
          <TSource, TDestination>()
      {
         // get matching mapping
         var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

         if (matchingMapping.Count() == 1)
         {
            return matchingMapping.First()._mappingDictionary;
         }

         throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}");
      }

      public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
      {
         var propertyMapping = GetPropertyMapping<TSource, TDestination>();

         if (string.IsNullOrWhiteSpace(fields))
         {
            return true;
         }

         // the string is separated by ",", so we split it.
         var fieldsAfterSplit = fields.Split(',');

         // run through the fields clauses
         foreach (var field in fieldsAfterSplit)
         {
            // trim
            var trimmedField = field.Trim();

            // remove everything after the first " " - if the fields 
            // are coming from an orderBy string, this part must be 
            // ignored
            var indexOfFirstSpace = trimmedField.IndexOf(" ");
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedField : trimmedField.Remove(indexOfFirstSpace);

            // find the matching property
            if (!propertyMapping.ContainsKey(propertyName))
            {
               return false;
            }
         }
         return true;
      }

   }
   

   public interface IPropertyMappingService
   {
      bool ValidMappingExistsFor<TSource, TDestination>(string fields);

      Dictionary<string, PropertyMappings> GetPropertyMapping<TSource, TDestination>();
   }

   public class PropertyMappings
   {
      public IEnumerable<string> DestinationProperties { get; set; }
      public bool Reverse { get; private set; }

      public PropertyMappings(IEnumerable<string> destinationProperties, bool reverse = false)
      {
         DestinationProperties = destinationProperties;
         Reverse = reverse;
      }
   }


   public interface IPropertyMapping
   {
   }
   public class PropertyMapping<TSource, TDestination> : IPropertyMapping
   {
      public Dictionary<string, PropertyMappings> _mappingDictionary { get; private set; }

      public PropertyMapping(Dictionary<string, PropertyMappings> mappingDictionary)
      {
         _mappingDictionary = mappingDictionary;
      }
   }
}
