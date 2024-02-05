using System;
using System.Collections.Generic;
using System.Text;

namespace aoaifunctest.ResponseEntities
{
    public class SkillResponse
    {
        public SkillResponse()
        {
            values = new List<Values>();
        }

        public List<Values> values { get; set; }
    }
}
