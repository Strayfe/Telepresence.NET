using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Telepresence.NET.InterceptSpec.Converters;

public class YamlStringEnumConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type.IsEnum;

    public object ReadYaml(IParser parser, Type type) =>
        throw new NotImplementedException();

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var enumMember = type
            .GetMember(value?.ToString() ?? string.Empty)
            .FirstOrDefault();

        var yamlValue = enumMember?
            .GetCustomAttributes<EnumMemberAttribute>(true)
            .Select(ema => ema.Value)
            .FirstOrDefault() ??
                        value?.ToString();

        emitter.Emit(new Scalar(yamlValue ?? string.Empty));
    }
}