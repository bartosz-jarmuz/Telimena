using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using DotNetLittleHelpers;
using FluentAssertions;
using NUnit.Framework;
using Telimena.WebApp;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;

namespace Telimena.Tests
{
    [TestFixture]
    public class MappingsTests
    {
        [Test]
        public void EnsureAutoMapperConfigIsValid()
        {
            AutoMapperConfiguration.Validate();
        }

        [Test]
        public void TestMappableObjects()
        {
            List<Type> webAppClasses = typeof(UpdateRequest).Assembly.GetTypes().Where(x => (x.IsClass || x.IsEnum)  && x.Namespace == typeof(UpdateRequest).Namespace).ToList();
            List<Type> clientClasses = typeof(TelimenaClient.Model.UpdateRequest).Assembly.GetTypes().Where(x => (x.IsClass || x.IsEnum) && x.Namespace == typeof(TelimenaClient.Model.UpdateRequest).Namespace).ToList();

            List<string> errors = new List<string>();

            this.PerformValidation(webAppClasses, clientClasses, errors);
            this.PerformValidation(clientClasses, webAppClasses, errors);

            if (errors.Any())
            {
                Assert.Fail(string.Join("\r\n", errors));
            }
        }

        private void PerformValidation(List<Type> sourceClasses, List<Type> targetClasses, List<string> errors)
        {
            foreach (Type sourceClass in sourceClasses)
            {
                Type targetClass = targetClasses.FirstOrDefault(x => x.Name == sourceClass.Name);
                if (targetClass == null)
                {
                    errors.Add($"Failed to find a counterpart of class [{sourceClass.FullName}]");
                    continue;
                }

                this.CompareProperties(sourceClass, targetClass, errors);
                this.CompareValues(sourceClass, targetClass, errors);
                this.CompareEnums(sourceClass, targetClass,errors);
            }
        }

        private void CompareValues(Type source, Type target, List<string> errors)
        {
            try
            {

                if (!source.IsAbstract)
                {

                    var sourceInstance = Activator.CreateInstance(source);
                    var targetInstance = Activator.CreateInstance(target);
                    // sourceInstance.ShouldBeEquivalentTo(targetInstance);

                }

            }
            catch (Exception ex)
            {
                errors.Add($"Error while trying to activate and compare {source.FullName} and {target.FullName}. {ex}");
            }
        }

        private void CompareEnums(Type source, Type target, List<string> errors)
        {
            if (source.IsEnum)
            {
                foreach (object value in Enum.GetValues(source))
                {
                    var stringified = value.ToString();
                    var numeric = (int)value ;

                    Assert.AreEqual(stringified, Enum.GetName(target, numeric));
                }
                foreach (object value in Enum.GetValues(target))
                {
                    var stringified = value.ToString();
                    var numeric = (int)value;

                    Assert.AreEqual(stringified, Enum.GetName(target, numeric));
                }
            }
        }

        private void CompareProperties(Type source, Type target, List<string> errors)
        {
            if (source.IsEnum)
            {
                return;
            }

            var allFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                           BindingFlags.FlattenHierarchy;

            foreach (var sourceMember in source.GetMembers(allFlags).Where(x=>x.MemberType != MemberTypes.Constructor))
            {
                try
                {
                    MemberInfo targetMember;
                    if (sourceMember.MemberType == MemberTypes.Method)
                    {
                        targetMember = target.GetMember(sourceMember.Name, allFlags).Where(x =>
                        {
                            var sourceParams = (sourceMember as MethodInfo).GetParameters();
                            var targetParams = (x as MethodInfo).GetParameters();
                            try
                            {
                                for (int index = 0; index < sourceParams.Length; index++)
                                {
                                    ParameterInfo sourceParam = sourceParams[index];
                                    ParameterInfo targetParam = targetParams[index];
                                    if (sourceParam.ParameterType.Name != targetParam.ParameterType.Name ||
                                        sourceParam.Name != targetParam.Name)
                                    {
                                        return false;
                                    }
                                }

                                return true;
                            }
                            catch
                            {
                                return false;
                            }

                        }).SingleOrDefault();
                    }
                    else
                    {
                        targetMember = target.GetMember(sourceMember.Name, allFlags).SingleOrDefault();
                    }


                    if (targetMember == null)
                    {
                        errors.Add(
                            $"Failed to find a counterpart of member [{sourceMember.Name}] from [{source.FullName}] in class [{target.FullName}]");
                        continue;
                    }

                    VerifyStaticMembers(sourceMember, targetMember);
                }
                catch (Exception ex)
                {
                    errors.Add(
                        $"Error while validating member [{sourceMember.Name}] from [{source.FullName}]in class [{target.FullName}] {ex}");

                }
            }
        }

        private static void VerifyStaticMembers(MemberInfo sourceMember, MemberInfo targetMember)
        {
            if (sourceMember is PropertyInfo propertyInfo)
            {
                var flags = propertyInfo.GetPropertyValue<BindingFlags>("BindingFlags");
                if (flags.HasFlag(BindingFlags.Static))
                {
                    var sourceValue = propertyInfo.GetValue(null);
                    var targetValue = (targetMember as PropertyInfo).GetValue(null);
                    Assert.AreEqual(sourceValue, targetValue);
                }
            }

            if (sourceMember is FieldInfo fieldInfo)
            {
                var flags = fieldInfo.GetPropertyValue<BindingFlags>("BindingFlags");
                if (flags.HasFlag(BindingFlags.Static))
                {
                    var sourceValue = fieldInfo.GetValue(null);
                    var targetValue = (targetMember as FieldInfo).GetValue(null);
                    Assert.AreEqual(sourceValue, targetValue);
                }
            }
        }
    }
}