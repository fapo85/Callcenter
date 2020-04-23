using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Text.Json;

namespace Callcenter.Models
{
    internal class TimeKapselConverter : MongoDB.Bson.Serialization.IBsonSerializer, MongoDB.Bson.Serialization.IBsonSerializer<TimeKapsel>
    {
        public Type ValueType => typeof(TimeKapsel);

        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            var type = reader.CurrentBsonType;
            switch (type)
            {
                case BsonType.Null:
                    context.Reader.ReadNull();
                    return null;
                case MongoDB.Bson.BsonType.Double:
                    return new TimeKapsel(BsonSerializer.Deserialize<double>(context.Reader));
                case MongoDB.Bson.BsonType.DateTime:
                    return new TimeKapsel(BsonSerializer.Deserialize<DateTime>(context.Reader));
                case MongoDB.Bson.BsonType.String:
                    return new TimeKapsel(BsonSerializer.Deserialize<String>(context.Reader));
                default:
                    var message = string.Format("Cannot deserialize from BsonType {0}.", type);
                    throw new BsonSerializationException(message);
            }
        }



        TimeKapsel IBsonSerializer<TimeKapsel>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            var type = reader.CurrentBsonType;
            switch (type)
            {
                case BsonType.Null:
                    context.Reader.ReadNull();
                    return null;
                case MongoDB.Bson.BsonType.Double:
                    return new TimeKapsel(BsonSerializer.Deserialize<double>(context.Reader));
                case MongoDB.Bson.BsonType.DateTime:
                    return new TimeKapsel(BsonSerializer.Deserialize<DateTime>(context.Reader));
                default:
                    var message = string.Format("Cannot deserialize from BsonType {0}.", type);
                    throw new BsonSerializationException(message);
            }
        }
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            if (value != null)
            {
                switch (value)
                {
                    case DateTime obj:
                        context.Writer.WriteString(obj.ToString());
                        break;
                    case TimeKapsel obj:
                        string json = JsonSerializer.Serialize<TimeKapsel>(obj);
                        context.Writer.WriteString(JsonSerializer.Serialize<TimeKapsel>(obj));
                        break;
                    default:
                        var message = string.Format("Cannot serilize from BsonType {0}.", value.GetType());
                        throw new BsonSerializationException(message);
                }
            }
            else
            {
                context.Writer.WriteNull();
            }
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeKapsel value)
        {
            if (value != null)
            {
                context.Writer.WriteString(((DateTime)value).ToString());
            }
            else
            {
                context.Writer.WriteNull();
            }
        }
    }
}