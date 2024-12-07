

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NoSqlDataAccess.Common.Extensions
{
    /// <summary>
    /// This class provides extension methods for JSON operations.
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Performs Null and empty check for JToken.
        /// </summary>
        /// <param name="token">JToken value</param>
        /// <returns>Flag which tells if JToken value is Null or Empty.</returns>
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }

        /// <summary>
        /// Generic method to get a type specific value from source JSON object.
        /// Works only for immediate properties on the source object.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="source">The source JSON object.</param>
        /// <param name="propertyName">The property name withing source.</param>
        /// <returns>The value of type T.</returns>
        public static T GetValue<T>(this JObject source, string propertyName)
        {
            if (!source.IsNullOrEmpty() && !source[propertyName].IsNullOrEmpty())
            {
                // Get the type of template type T
                Type typeParameterType = typeof(T);

                // Get the type of Template type T, this works when requested type is nullable
                typeParameterType = Nullable.GetUnderlyingType(typeParameterType) ?? typeParameterType;

                // Resolve value
                return (T)Convert.ChangeType(source.Value<T>(propertyName), typeParameterType);
            }
            return default(T);
        }

        /// <summary>
        /// Builds a <see cref="JObject"/> from source object.
        /// JSON keys follow the camelCase naming format.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>An instance of <see cref="JObject"/>.</returns>
        public static JObject ConvertToJObjectCc(this object source)
        {
            JObject result = null;
            if (source != null)
            {
                result = JObject.FromObject(source, new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }

            return result;
        }

        /// <summary>
        /// Returns a flag to indicate whether given JSON token value is a complex type.
        /// Values of object and array type are trated as complex values.
        /// </summary>
        /// <param name="value">The input <see cref="JToken"/> value.</param>
        /// <returns>True if the token value is complex type, false otherwise.</returns>
        public static bool IsComplexValue(this JToken value)
        {
            var isComplexValue = false;
            if (!value.IsNullOrEmpty())
            {
                isComplexValue = (value.Type == JTokenType.Object) || (value.Type == JTokenType.Array);
            }

            return isComplexValue;
        }
    }

    /// <summary>
    /// Special JsonConvert resolver that allows you to ignore properties.
    /// </summary>
    public class IgnorableSerializerContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Dictionary to maintain ignored properties for specific types.
        /// </summary>
        protected readonly Dictionary<Type, HashSet<string>> Ignores;

        /// <summary>
        /// Constructor to initialize ignored properties for specific types.
        /// </summary>
        /// <param name="ignores">Dictionary where keys are types and values are property names to ignore.</param>
        public IgnorableSerializerContractResolver(Dictionary<Type, string[]> ignores)
        {
            this.Ignores = new Dictionary<Type, HashSet<string>>();

            // Populate the Ignores dictionary based on the constructor input
            foreach (var kvp in ignores)
            {
                this.Ignores[kvp.Key] = new HashSet<string>(kvp.Value);
            }
        }

        /// <summary>
        /// Is the given property for the given type ignored?
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property is ignored; otherwise, false.</returns>
        public bool IsIgnored(Type type, string propertyName)
        {
            if (!this.Ignores.ContainsKey(type)) return false;

            // If no properties provided, ignore the type entirely
            if (this.Ignores[type].Count == 0) return true;

            return this.Ignores[type].Contains(propertyName);
        }

        /// <summary>
        /// The decision logic goes here.
        /// </summary>
        /// <param name="member">The member information.</param>
        /// <param name="memberSerialization">The member serialization options.</param>
        /// <returns>The created JsonProperty.</returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (this.IsIgnored(property.DeclaringType, property.PropertyName)
                || this.IsIgnored(property.DeclaringType.BaseType, property.PropertyName))
            {
                property.ShouldSerialize = instance => false; 
            }

            return property;
        }
    }
}
