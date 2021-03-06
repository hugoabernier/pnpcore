﻿using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace PnP.Core.QueryModel.Model
{

    internal static class QueryableDataModels
    {
        public static Type[] Supported
        {
            get => new Type[]
            {
                typeof(Core.Model.SharePoint.IList),
                typeof(Core.Model.SharePoint.IListItem),
                typeof(Core.Model.SharePoint.IContentType),
                typeof(Core.Model.SharePoint.IField),
                typeof(Core.Model.SharePoint.IWeb),
                typeof(Core.Model.Teams.ITeamChannel),
                typeof(Core.Model.Teams.ITeamChatMessage)
            };
        }

        public static bool IsSupportedQueryableModel<TModel>()
        {
            return Supported.Any(s => typeof(TModel).IsAssignableFrom(s));
        }
    }

    /// <summary>
    /// Base type for any LINQ IQueryable collection of the Domain Model
    /// In the real model, could inherit from  BaseDataModelCollection<TModel>
    /// and implement IQueryable<TModel>
    /// </summary>
    /// <typeparam name="TModel">The Type of the collection</typeparam>
    internal class QueryableDataModelCollection<TModel> : BaseQueryableDataModelCollection<TModel>
    {
        #region Constructors

        // Keep this constructor protected to avoid creation from outside
        protected QueryableDataModelCollection(PnPContext context, IDataModelParent parent, string memberName = null)
        {
            var queryService = new DataModelQueryService<TModel>(context, parent, memberName);
            this.provider = new DataModelQueryProvider<TModel>(queryService);
            this.Expression = Expression.Constant(this);
        }

        // Public constructor that accepts a LINQ Query Provider object instance
        // it uses the current object as a constant LINQ Expression
        public QueryableDataModelCollection(DataModelQueryProvider<TModel> provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.Expression = Expression.Constant(this);
        }

        // Public constructor that accepts a LINQ Query Provider object instance
        // it uses the provided expression as the LINQ Expression
        public QueryableDataModelCollection(DataModelQueryProvider<TModel> provider, Expression expression)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.Expression = expression;
        }

        #endregion

        /// <summary>
        /// Returns OData query string for the current expression
        /// </summary>
        public override string ToString()
        {
            return provider.Translate(Expression).ToString();
        }
    }
}
