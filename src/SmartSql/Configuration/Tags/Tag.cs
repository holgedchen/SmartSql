﻿using SmartSql.Exceptions;
using System;
using System.Collections.Generic;

namespace SmartSql.Configuration.Tags
{
    public abstract class Tag : ITag
    {
        public virtual String Prepend { get; set; }
        public String Property { get; set; }
        public bool Required { get; set; }
        public IList<ITag> ChildTags { get; set; }
        public ITag Parent { get; set; }
        public Statement Statement { get; set; }

        public abstract bool IsCondition(RequestContext context);
        public virtual void BuildSql(RequestContext context)
        {
            if (IsCondition(context))
            {
                context.SqlBuilder.Append(" ");
                if (!context.IgnorePrepend)
                {
                    context.SqlBuilder.Append(Prepend);
                }
                else
                {
                    context.IgnorePrepend = false;
                }
                context.SqlBuilder.Append(" ");
                BuildChildSql(context);
            }
        }

        public virtual void BuildChildSql(RequestContext context)
        {
            if (ChildTags == null || ChildTags.Count <= 0) return;
            foreach (var childTag in ChildTags)
            {
                childTag.BuildSql(context);
            }
        }
        protected virtual String GetDbProviderPrefix(RequestContext context)
        {
            return context.ExecutionContext.SmartSqlConfig.Database.DbProvider.ParameterPrefix;
        }
        protected virtual object EnsurePropertyValue(RequestContext context)
        {
            var existProperty = context.Parameters.TryGetParameterValue(Property, out object paramVal);
            if (Required && !existProperty)
            {
                throw new TagRequiredFailException(this);
            }
            return paramVal;
        }
    }
}
