﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Chandler.Data.Entities
{
    [Table("posts")]
    public class Post
    {
        [Column("id")]
        public ulong Id { get; set; }

        [Column("threadid")]
        public ulong ThreadId { get; set; }

        [Column("image")]
        public string Image { get; set; } = "";

        [Column("Username")]
        public string Username { get; set; } = "Anonymous";

        [Column("text")]
        public string Text { get; set; }

        [Column("ip")]
        public string Ip { get; set; }
    }
}
