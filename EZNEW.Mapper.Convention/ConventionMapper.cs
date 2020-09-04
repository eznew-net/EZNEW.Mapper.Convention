using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using EZNEW.Application;
using EZNEW.Configuration;
using EZNEW.Develop.Domain;
using EZNEW.Develop.Entity;
using EZNEW.Logging;

namespace EZNEW.Mapper.Convention
{
    /// <summary>
    /// Convention mapper
    /// </summary>
    public static class ConventionMapper
    {
        /// <summary>
        /// Mapper configuration action
        /// </summary>
        static Action<IMapperConfigurationExpression> mapperConfigurationAction;

        static ConventionMapper()
        {
            mapperConfigurationAction = new Action<IMapperConfigurationExpression>(ConfigureConventionMap);
        }

        /// <summary>
        /// Configure convention map
        /// </summary>
        /// <param name="mapperConfigurationExpression">Mapper configuration</param>
        static void ConfigureConventionMap(IMapperConfigurationExpression mapperConfigurationExpression)
        {
            IEnumerable<FileInfo> files = new DirectoryInfo(ApplicationManager.ApplicationExecutableDirectory).GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(c => !ConfigurationOptions.ConfigurationExcludeFileRegex.IsMatch(c.FullName)) ?? Array.Empty<FileInfo>();
            List<Type> allTypes = new List<Type>();
            foreach (var file in files)
            {
                try
                {
                    allTypes.AddRange(Assembly.LoadFrom(file.FullName).GetTypes());
                }
                catch (Exception ex)
                {
                    LogManager.LogError(ex, ex.Message);
                }
            }
            var allEntitys = EntityManager.GetAllEntityConfigurations()?.Select(c => c.EntityType) ?? Array.Empty<Type>();
            var defaultMemberValidation = MemberList.None;
            foreach (var entity in allEntitys)
            {
                var entityName = entity.Name;
                if (entityName.EndsWith("Entity"))
                {
                    entityName = entityName.Substring(0, entityName.Length - 6);
                }

                //domain
                var domainType = allTypes.FirstOrDefault(c => c.Name == entityName && c != entity);
                if (domainType != null)
                {
                    mapperConfigurationExpression.CreateMap(entity, domainType, defaultMemberValidation);
                    mapperConfigurationExpression.CreateMap(domainType, entity, defaultMemberValidation);
                }
                //dto
                var dtoType = allTypes.FirstOrDefault(c => c.Name == $"{entityName}Dto");
                if (dtoType != null && domainType != null)
                {
                    mapperConfigurationExpression.CreateMap(domainType, dtoType, defaultMemberValidation);
                    mapperConfigurationExpression.CreateMap(dtoType, domainType, defaultMemberValidation);
                }
                //view model
                var viewModelType = allTypes.FirstOrDefault(c => c.Name == $"{entityName}ViewModel");
                if (dtoType != null && viewModelType != null)
                {
                    mapperConfigurationExpression.CreateMap(viewModelType, dtoType, defaultMemberValidation);
                    mapperConfigurationExpression.CreateMap(dtoType, viewModelType, defaultMemberValidation);
                }
                //edit view model
                var editViewModelType = allTypes.FirstOrDefault(c => c.Name == $"Edit{entityName}ViewModel");
                if (editViewModelType != null && dtoType != null)
                {
                    mapperConfigurationExpression.CreateMap(editViewModelType, dtoType, defaultMemberValidation);
                    mapperConfigurationExpression.CreateMap(dtoType, editViewModelType, defaultMemberValidation);
                }
                if (viewModelType != null && editViewModelType != null)
                {
                    mapperConfigurationExpression.CreateMap(editViewModelType, viewModelType, defaultMemberValidation);
                    mapperConfigurationExpression.CreateMap(viewModelType, editViewModelType, defaultMemberValidation);
                }
            }
            var domainParameterContract = typeof(IDomainParameter);
            //parmater
            var domainParameterTypes = allTypes.Where(c => domainParameterContract.IsAssignableFrom(c));
            foreach (var parameterType in domainParameterTypes)
            {
                var parameterName = parameterType.Name.LSplit("Parameter")[0];
                //parameter dto
                var parameterDto = allTypes.FirstOrDefault(c => c.Name == $"{parameterName}Dto");
                if (parameterDto != null)
                {
                    mapperConfigurationExpression.CreateMap(parameterType, parameterDto, defaultMemberValidation);
                    mapperConfigurationExpression.CreateMap(parameterDto, parameterType, defaultMemberValidation);
                }
                // parameter viewmodel
                var parameterViewModel = allTypes.FirstOrDefault(c => c.Name == $"{parameterName}ViewModel");
                if (parameterViewModel != null && parameterDto != null)
                {
                    mapperConfigurationExpression.CreateMap(parameterViewModel, parameterDto, defaultMemberValidation);
                    mapperConfigurationExpression.CreateMap(parameterDto, parameterViewModel, defaultMemberValidation);
                }
            }
        }

        /// <summary>
        /// Configure map
        /// </summary>
        /// <param name="configureAction">Configure action</param>
        public static void ConfigureMap(Action<IMapperConfigurationExpression> configureAction)
        {
            if (configureAction != null)
            {
                mapperConfigurationAction += configureAction;
            }
        }

        /// <summary>
        /// Create a EZNEW.Mapper.IMapper instance
        /// </summary>
        /// <returns>Return a new EZNEW.Mapper.IMapper instance</returns>
        public static IMapper CreateMapper(Action<IMapperConfigurationExpression> configureAction = null, bool coverGlobalMapper = true)
        {
            var mapperConfigureAction = new Action<IMapperConfigurationExpression>(mapperConfigurationAction);
            if (configureAction != null)
            {
                mapperConfigureAction += configureAction;
            }
            var mapper = new AutoMapMapper(mapperConfigureAction);
            if (coverGlobalMapper)
            {
                ObjectMapper.Current = mapper;
            }
            return mapper;
        }
    }
}
