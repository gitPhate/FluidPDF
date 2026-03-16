using FluentAssertions;
using FluidPDF.Support.Json;
using System.Dynamic;
using System.Text;
using System.Text.Json;

namespace FluidPDF.Tests
{
    public class Item
    {
        public int Id { get; set; }
        public int Value { get; set; }
    }

    public class ExpandoObjectConverterTests
    {
        // ExpandoObjectConverter Tests

        [Fact]
        public void ExpandoObjectConverter_Write_SerializesConcreteTypes()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream);
            Item item = new() { Id = 1, Value = 2 };

            // Act
            converter.Write(writer, item, options);
            writer.Flush();

            // Assert
            stream.Position = 0;
            string result = Encoding.UTF8.GetString(stream.ToArray());
            result.Should().Contain("\"Id\":1");
            result.Should().Contain("\"Value\":2");
        }

        [Fact]
        public void ExpandoObjectConverter_Write_PlainObject_WritesEmptyJsonObject()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream);
            object value = new();

            // Act
            converter.Write(writer, value, options);
            writer.Flush();

            // Assert
            stream.Position = 0;
            string result = Encoding.UTF8.GetString(stream.ToArray());
            result.Should().Be("{}");
        }

        [Fact]
        public void ExpandoObjectConverter_Read_NullToken_ReturnsNull()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            string json = "null";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void ExpandoObjectConverter_Read_BooleanToken_ReturnsCorrectValue(string json, bool expected)
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ExpandoObjectConverter_Read_RegularString_ReturnsString()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            string json = "\"hello\"";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            // Assert
            result.Should().Be("hello");
        }

        [Fact]
        public void ExpandoObjectConverter_Read_DateTimeString_ReturnsDateTime()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            string json = "\"2023-01-01T00:00:00\"";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            // Assert
            result.Should().BeOfType<DateTime>();
        }

        [Fact]
        public void ExpandoObjectConverter_Read_GuidString_ReturnsGuid()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            string json = "\"00000000-0000-0000-0000-000000000000\"";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            // Assert
            result.Should().BeOfType<Guid>();
        }

        [Fact]
        public void ExpandoObjectConverter_Read_NumberTokens_DoubleFormat_ReturnsCorrectTypes()
        {
            // Arrange
            ExpandoObjectConverter converter = new(FloatFormat.Double, UnknownNumberFormat.Error, ObjectFormat.Expando);
            JsonSerializerOptions options = new();

            // Test integer
            string intJson = "42";
            Utf8JsonReader intReader = new(Encoding.UTF8.GetBytes(intJson));
            intReader.Read();
            object intResult = converter.Read(ref intReader, typeof(object), options);

            // Test long
            string longJson = "9223372036854775807";
            Utf8JsonReader longReader = new(Encoding.UTF8.GetBytes(longJson));
            longReader.Read();
            object longResult = converter.Read(ref longReader, typeof(object), options);

            // Test double
            string doubleJson = "3.14";
            Utf8JsonReader doubleReader = new(Encoding.UTF8.GetBytes(doubleJson));
            doubleReader.Read();
            object doubleResult = converter.Read(ref doubleReader, typeof(object), options);

            // Assert
            intResult.Should().Be(42);
            longResult.Should().Be(9223372036854775807L);
            doubleResult.Should().Be(3.14);
        }

        [Fact]
        public void ExpandoObjectConverter_Read_NumberTokens_DecimalFormat_ReturnsDecimal()
        {
            // Arrange
            ExpandoObjectConverter converter = new(FloatFormat.Decimal, UnknownNumberFormat.Error, ObjectFormat.Expando);
            JsonSerializerOptions options = new();

            string decimalJson = "3.14";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(decimalJson));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<decimal>();
            result.Should().Be(3.14m);
        }

        [Fact]
        public void ExpandoObjectConverter_Read_NumberTokens_UnknownNumberFormat_JsonElement_ReturnsJsonElement()
        {
            // Arrange — UnknownNumberFormat.JsonElement is only reachable with FloatFormat.Decimal
            // when the number overflows decimal range (TryGetInt32, TryGetInt64, TryGetDecimal all fail).
            // TryGetDouble always succeeds for valid JSON numbers, so FloatFormat.Double cannot reach this path.
            ExpandoObjectConverter converter = new(FloatFormat.Decimal, UnknownNumberFormat.JsonElement, ObjectFormat.Expando);

            byte[] bytes = Encoding.UTF8.GetBytes("1e99999"); // too large for decimal
            Utf8JsonReader reader = new(bytes, new JsonReaderOptions());
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), new JsonSerializerOptions());

            // Assert
            result.Should().BeOfType<JsonElement>();
        }

        [Fact]
        public void ExpandoObjectConverter_Read_NumberTokens_UnknownNumberFormat_Error_ThrowsJsonException()
        {
            // Arrange — FloatFormat.Decimal + UnknownNumberFormat.Error: a number that overflows decimal
            // range will exhaust all TryGetXxx calls and hit the Error throw path.
            ExpandoObjectConverter converter = new(FloatFormat.Decimal, UnknownNumberFormat.Error, ObjectFormat.Expando);

            byte[] bytes = Encoding.UTF8.GetBytes("1e99999");

            // Act & Assert — can't capture ref local in lambda, so assert inline
            JsonException? thrown = null;
            try
            {
                Utf8JsonReader reader = new(bytes, new JsonReaderOptions());
                reader.Read();
                converter.Read(ref reader, typeof(object), new JsonSerializerOptions());
            }
            catch (JsonException ex)
            {
                thrown = ex;
            }
            thrown.Should().NotBeNull("converter should throw JsonException for unparsable number with UnknownNumberFormat.Error");
        }

        [Fact]
        public void ExpandoObjectConverter_Read_ArrayTokens_ReturnsArray()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            string json = "[1, 2, 3]";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<int[]>();
            int[] array = (int[])result;
            array.Should().HaveCount(3);
            array[0].Should().Be(1);
            array[1].Should().Be(2);
            array[2].Should().Be(3);
        }

        [Fact]
        public void ExpandoObjectConverter_Read_EmptyArray_ReturnsEmptyArray()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            string json = "[]";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<object[]>();
            object[] array = (object[])result;
            array.Should().BeEmpty();
        }

        [Fact]
        public void ExpandoObjectConverter_Read_MixedTypeArray_ThrowsJsonException()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            string json = "[1, \"hello\"]";

            // Act & Assert — can't capture ref local in lambda, so assert inline
            JsonException? thrown = null;
            try
            {
                Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
                reader.Read();
                converter.Read(ref reader, typeof(object), options);
            }
            catch (JsonException ex)
            {
                thrown = ex;
            }
            thrown.Should().NotBeNull("converter should throw JsonException for mixed-type arrays");
            thrown!.Message.Should().Contain("mixed element types");
        }

        [Fact]
        public void ExpandoObjectConverter_Read_ObjectTokens_ExpandoFormat_ReturnsExpandoObject()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            string json = "{\"name\":\"John\"}";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<ExpandoObject>();
            dynamic expando = (ExpandoObject)result;
            string nameValue = (string)expando.name;
            nameValue.Should().Be("John");
        }

        [Fact]
        public void ExpandoObjectConverter_Read_ObjectTokens_DictionaryFormat_ReturnsDictionary()
        {
            // Arrange
            ExpandoObjectConverter converter = new(FloatFormat.Double, UnknownNumberFormat.Error, ObjectFormat.Dictionary);
            JsonSerializerOptions options = new();
            string json = "{\"name\":\"John\",\"age\":30}";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<Dictionary<string, object>>();
            Dictionary<string, object> dict = (Dictionary<string, object>)result;
            dict["name"].Should().Be("John");
            dict["age"].Should().Be(30);
        }

        [Fact]
        public void ExpandoObjectConverter_Read_EmptyObject_ReturnsEmptyExpando()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            string json = "{}";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<ExpandoObject>();
            ExpandoObject expando = (ExpandoObject)result;
            expando.Should().BeEmpty();
        }

        [Fact]
        public void ExpandoObjectConverter_Read_NestedObject_HandlesNestingCorrectly()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            string json = "{\"person\":{\"name\":\"John\",\"details\":{\"age\":30,\"city\":\"NYC\"}}}";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<ExpandoObject>();
            dynamic expando = (ExpandoObject)result;
            ((ExpandoObject)expando.person).Should().BeOfType<ExpandoObject>();
            ((string)expando.person.name).Should().Be("John");
            ((ExpandoObject)expando.person.details).Should().BeOfType<ExpandoObject>();
            ((int)expando.person.details.age).Should().Be(30);
            ((string)expando.person.details.city).Should().Be("NYC");
        }

        [Fact]
        public void ExpandoObjectConverter_Read_NestedArray_HandlesNestingCorrectly()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            string json = "{\"items\":[{\"id\":1,\"name\":\"item1\"},{\"id\":2,\"name\":\"item2\"}]}";
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(json));
            reader.Read();

            // Act
            object result = converter.Read(ref reader, typeof(object), options);

            // Assert
            result.Should().BeOfType<ExpandoObject>();
            dynamic expando = (ExpandoObject)result;
            ExpandoObject[] items = (ExpandoObject[])expando.items;
            items.Should().HaveCount(2);

            dynamic item1 = items[0];
            ((int)item1.id).Should().Be(1);
            ((string)item1.name).Should().Be("item1");

            dynamic item2 = items[1];
            ((int)item2.id).Should().Be(2);
            ((string)item2.name).Should().Be("item2");
        }

        [Fact]
        public void ExpandoObjectConverter_Read_InvalidToken_ThrowsJsonException()
        {
            // Arrange — position reader at EndObject, which is not a valid start token
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();

            // Act & Assert
            Action act = () => {
                Utf8JsonReader testReader = new(Encoding.UTF8.GetBytes("{}"));
                testReader.Read(); // StartObject
                testReader.Read(); // EndObject
                converter.Read(ref testReader, typeof(object), options);
            };
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void ExpandoObjectConverter_FullRoundTrip_SerializationDeserialization()
        {
            // Arrange
            ExpandoObjectConverter converter = new();
            JsonSerializerOptions options = new();
            options.Converters.Add(converter);

            dynamic original = new ExpandoObject();
            original.Name = "John";
            original.Age = 30;
            original.IsActive = true;
            original.Scores = new[] { 95, 87, 92 };
            original.Address = new ExpandoObject();
            original.Address.Street = "123 Main St";
            original.Address.City = "NYC";

            // Act
            var json = JsonSerializer.Serialize(original, options);
            dynamic deserialized = JsonSerializer.Deserialize<ExpandoObject>(json, options);

            // Assert
            ((string)deserialized.Name).Should().Be(original.Name);
            ((int)deserialized.Age).Should().Be(original.Age);
            ((bool)deserialized.IsActive).Should().Be(original.IsActive);
            ((int[])deserialized.Scores).Should().HaveCount(original.Scores.Length);
            ((string)deserialized.Address.Street).Should().Be(original.Address.Street);
            ((string)deserialized.Address.City).Should().Be(original.Address.City);
        }
    }
}
