using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class SkillInput
    {
        public List<Data> values;
    }

    public class Data
    {
        public List<DataValues> data;
    }

    public class DataValues
    {
        public string text;
        public List<string> words;
    }

    public class SkillOutput
    {
        public List<OutputValues> values;
    }

    public class OutputValues
    {
        public class OutputData
        {
            public string name;
            public string matchIndex;
        }

        public string recordid;
        public List<OutputData> data;
    }
}
