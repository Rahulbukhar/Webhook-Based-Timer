

using NoSqlDataAccess.Common.Constants;
using NoSqlDataAccess.Common.Query.Model;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlDataAccess.Common.Model
{
    /// <summary>
    /// Data structure to represent input to clone table(s).
    /// </summary>
    public class CloneInput
    {
        /// <summary>
        /// An instance of <see cref="List{CloneTableStatus}"/>. Useful to resume from last known clone state.
        /// </summary>
        public List<CloneTableStatus> CloneTableStatuses { get; set; }

        /// <summary>
        /// Maximum number of objects to clone per table, at once.
        /// </summary>
        public int BatchSize { get; set; } = DbConstants.CLONE_MAX_OBJECTS_COUNT_PER_TABLE;

        /// <summary>
        /// Clone Cut-off criteria.
        /// Only the range of items satisfying this criteria will be cloned.
        /// </summary>
        public SearchExpression CutOffCriteria { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tableNames"></param>
        /// <param name="cutOffCriteria">An instance of <see cref="SearchExpression"/> that specifies the cut-off criteria.</param>
        public CloneInput(List<string> tableNames, SearchExpression cutOffCriteria)
        {
            if (tableNames != null)
            {
                CloneTableStatuses = new List<CloneTableStatus>();
                CloneTableStatuses.AddRange(tableNames.Select(tn => new CloneTableStatus { TableName = tn }).ToList());
                CutOffCriteria = cutOffCriteria;
            }
        }
    }
}
