

using Newtonsoft.Json;
using System;

namespace NoSqlDataAccess.Common.Query.Model
{
    /// <summary>
    /// Data structure for search criteria.
    /// </summary>
    public abstract class SearchExpression
    {
        /// <summary>
        /// Query operator
        /// </summary>
        protected QueryOperator op;

        /// <summary>
        /// Parent term to be prefixed to child properties.
        /// </summary>
        protected QueryTerm parentTerm { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="op">And instance of <seealso cref="QueryOperator"/></param>
        protected SearchExpression(QueryOperator op)
        {
            this.op = op;
        }
        /// <summary>
        /// Logical/Relational query operators.
        /// </summary>
        [JsonProperty("op")]
        public QueryOperator Op
        {
            get
            {
                return op;
            }
        }
        /// <summary>
        /// Parent field in query criteria (Optional).
        /// </summary>
        public virtual QueryTerm ParentTerm
        {
            get
            {
                return parentTerm;
            }

            set
            {
                parentTerm = value;
            }
        }

        /// <summary>
        /// Validates QueryTerm - Synchronous
        /// </summary>
        /// <param name="validateAction">An Action delegate to perform validation over Left and Right Query Terms.</param>
        public abstract void Validate(Action<QueryTerm, QueryTerm, QueryOperator> validateAction);
    }

    /// <summary>
    /// Data-structure for Logical And, Or search criteria.
    /// </summary>
    public class LogicalSearchExpression : SearchExpression
    {
        private readonly SearchExpression[] searchTerms;

        /// <summary>
        /// Constructor
        /// </summary>
        public LogicalSearchExpression(QueryOperator op, SearchExpression[] searchTerms) : base(op)
        {
            this.searchTerms = searchTerms;
        }

        /// <summary>
        /// Input for the search criteria.
        /// </summary>
        [JsonProperty("searchTerms")]
        public SearchExpression[] SearchTerms
        {
            get
            {
                return searchTerms;
            }
        }

        /// <summary>
        /// Parent field in query criteria (Optional).
        /// </summary>
        public override QueryTerm ParentTerm
        {
            set
            {
                foreach (var searchTerm in searchTerms)
                {
                    searchTerm.ParentTerm = value;
                }
                parentTerm = value;
            }
        }

        /// <summary>
        /// Validates QueryTerm
        /// </summary>
        /// <param name="validateAction">An Action delegate to perform validation over Left and Right Query Terms.</param>
        public override void Validate(Action<QueryTerm, QueryTerm, QueryOperator> validateAction)
        {
            foreach (var searchTerm in searchTerms)
            {
                searchTerm.Validate(validateAction);
            }
        }
    }

    /// <summary>
    /// Data-structure for And search criteria.
    /// </summary>
    public class AndSearchExpression : LogicalSearchExpression
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchTerms">Array of search terms.</param>
        public AndSearchExpression(SearchExpression[] searchTerms) : base(QueryOperator.And, searchTerms)
        {
        }
    }

    /// <summary>
    /// Data-structure for Or search criteria.
    /// </summary>
    public class OrSearchExpression : LogicalSearchExpression
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="searchTerms">Array of search terms</param>
        public OrSearchExpression(SearchExpression[] searchTerms) : base(QueryOperator.Or, searchTerms)
        {
        }
    }


    /// <summary>
    /// Data-structure for Binary search criteria.
    /// </summary>
    public class BinarySearchExpression : SearchExpression
    {
        private readonly QueryTerm left;
        private readonly QueryTerm right;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="op">Query Operator</param>
        /// <param name="left">Left Operator</param>
        /// <param name="right">Right Operator</param>
        public BinarySearchExpression(QueryOperator op, QueryTerm left, QueryTerm right) : base(op)
        {
            this.left = left;
            this.right = right;
        }
        /// <summary>
        /// Represents the data structure for simple query condition composed of left operand. 
        /// </summary>
        [JsonProperty("left")]
        public QueryTerm Left
        {
            get
            {
                return left;
            }
        }

        /// <summary>
        /// Represents the data structure for simple query condition composed of right operand.
        /// </summary>
        [JsonProperty("right")]
        public QueryTerm Right
        {
            get
            {
                return right;
            }
        }
        /// <summary>
        /// Parent field in query criteria (Optional).
        /// </summary>
        public override QueryTerm ParentTerm
        {
            set
            {
                parentTerm = value;
            }
        }

        /// <summary>
        /// Validates QueryTerm
        /// </summary>
        /// <param name="validateAction">An Action delegate to perform validation over Left and Right Query Terms.</param>
        public override void Validate(Action<QueryTerm, QueryTerm, QueryOperator> validateAction)
        {
            if (parentTerm != null)
            {
                validateAction($"{parentTerm}.{left}", right, Op);
            }
            else
            {
                validateAction(left, right, Op);
            }
        }

        /// <summary>
        /// Overriding ToString method for debugging purpose
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Left != null && Left != QueryTerm.Null)
            {
                if (Right == null || Right == QueryTerm.Null)
                {
                    return $"{Left} {Op} null";
                }
                return $"{Left} {Op} {Right}";
            }
            return string.Empty;
        }
    }
}
