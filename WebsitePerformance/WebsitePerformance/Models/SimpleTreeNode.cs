using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsitePerformance.Models
{
    public class SimpleTreeNode
    {
        private static int id; 
        public int Id { get; set; }
        public string Title { get; set; }
        public string Ping { get; set; }
        public int? Parent_Id { get; set; }
        public string LVL { get; set; }

        public SimpleTreeNode()
        {
            Id = id++;
            Parent_Id = null;
        }

        public SimpleTreeNode(string title,string lvl,int? parentId = null):this()
        {
            Title = title;
            LVL = lvl;
            Parent_Id = parentId;
        }

        public override string ToString()
        {
            return $"{Id}";
        }

        public static void Refresh()
        {
            id = 0;
        }
    }
}