using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CourseLibrary.API.Helpers;

public class ArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // our binder works only on enumerable types
        if (!bindingContext.ModelMetadata.IsEnumerableType)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // get the inputted value through the value provider
        var value = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName)
            .ToString();

        // if that value is null or white space , we return null
        if (string.IsNullOrWhiteSpace(value))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // the value isn't null or white space ,
        // and the type of the model is enumerable 
        // get the Enumerable's type , and a converter
        var elementType = bindingContext.ModelType
            .GetTypeInfo()
            .GenericTypeArguments[0];

        var converter = TypeDescriptor.GetConverter(elementType);

        // Convert Each item in teh value list to enumerable type
        var values = value.Split(
                new [] {","}, 
                StringSplitOptions.RemoveEmptyEntries)
            .Select(x => converter.ConvertFromString(x.Trim()))
            .ToArray();

        // create an array of that type and set it as the model value
        var typedValues = Array.CreateInstance(elementType, values.Length);
        values.CopyTo(typedValues , 0);
        bindingContext.Model = typedValues;

        // return a successful result , passing in the model 
        bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
        return Task.CompletedTask;
    }
}