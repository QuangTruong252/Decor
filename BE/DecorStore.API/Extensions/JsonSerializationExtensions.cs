using System.Text.Json;
using System.Text.Json.Serialization;

namespace DecorStore.API.Extensions
{
    /// <summary>
    /// Extension methods for configuring JSON serialization services
    /// </summary>
    public static class JsonSerializationExtensions
    {
        /// <summary>
        /// Configures optimized JSON serialization options
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddOptimizedJsonSerialization(this IServiceCollection services)
        {
            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                var jsonOptions = options.SerializerOptions;
                
                // Performance optimizations
                jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.WriteIndented = false; // Compact JSON for production
                jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                
                // Handle enums as strings for better API usability
                jsonOptions.Converters.Add(new JsonStringEnumConverter());
                
                // Custom converters for optimized serialization
                jsonOptions.Converters.Add(new DateTimeJsonConverter());
                jsonOptions.Converters.Add(new DecimalJsonConverter());
                
                // Performance settings
                jsonOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                jsonOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                jsonOptions.AllowTrailingCommas = true;
                  // Security settings
                jsonOptions.MaxDepth = 64; // Prevent deep object graphs
                  // Configure reference handling to prevent circular references
                jsonOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                
                // Enable performance optimizations
                jsonOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
            });

            // Configure JSON options for MVC
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                var jsonOptions = options.JsonSerializerOptions;
                
                // Apply same optimizations to MVC JSON
                jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                jsonOptions.WriteIndented = false;
                jsonOptions.AllowTrailingCommas = true;
                jsonOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                jsonOptions.Converters.Add(new JsonStringEnumConverter());
                jsonOptions.Converters.Add(new DateTimeJsonConverter());
                jsonOptions.Converters.Add(new DecimalJsonConverter());
                jsonOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                jsonOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                jsonOptions.MaxDepth = 64;                jsonOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
            });

            return services;
        }
    }

    /// <summary>
    /// Custom DateTime converter for optimal JSON serialization
    /// </summary>
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

    /// <summary>
    /// Custom decimal converter for precise JSON serialization
    /// </summary>
    public class DecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return decimal.Parse(reader.GetString()!);
            }
            return reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
