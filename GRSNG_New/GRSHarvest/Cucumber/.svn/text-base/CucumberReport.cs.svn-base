// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GRSHarvest.Cucumber
{
    public class Result
    {
        public object duration { get; set; }
        public string status { get; set; }
        public string error_message { get; set; }
    }

    public class Match
    {
        public List<Argument> arguments { get; set; }
        public string location { get; set; }
    }

    public class Before
    {
        public Result result { get; set; }
        public Match match { get; set; }
    }

    public class After
    {
        public Result result { get; set; }
        public Match match { get; set; }
    }

    public class Argument
    {
        public string val { get; set; }
        public int offset { get; set; }
    }

    public class Embedding
    {
        public string data { get; set; }
        public string mime_type { get; set; }
    }

    public class Step
    {
        public Result result { get; set; }
        public int line { get; set; }
        public string name { get; set; }
        public Match match { get; set; }
        public string keyword { get; set; }
        public List<Embedding> embeddings { get; set; }
        public List<string> output { get; set; }
    }

    public class Tag
    {
        public int line { get; set; }
        public string name { get; set; }
    }

    public class Element
    {
        public List<Before> before { get; set; }
        public int line { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public List<After> after { get; set; }
        public string type { get; set; }
        public string keyword { get; set; }
        public List<Step> steps { get; set; }
        public List<Tag> tags { get; set; }
    }

    public class CucumberReport
    {
        public int line { get; set; }
        public List<Element> elements { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public string keyword { get; set; }
        public string uri { get; set; }
        public List<Tag> tags { get; set; }
    }
}
