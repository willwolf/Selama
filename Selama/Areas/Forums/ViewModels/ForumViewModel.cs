﻿using Selama.Areas.Forums.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Selama.Areas.Forums.ViewModels
{
    public class ForumViewModel
    {
        public ForumViewModel(Forum f)
        {
            Title = f.Title;
            SubTitle = f.SubTitle;
            Threads = new List<ThreadOverviewViewModel>();
            foreach (Thread t in f.Threads)
            {
                ((List<ThreadOverviewViewModel>)Threads).Add(new ThreadOverviewViewModel(t));
            }
        }

        public string Title { get; set; }

        [Display(Name = "Subtitle")]
        public string SubTitle { get; set; }

        public IEnumerable<ThreadOverviewViewModel> Threads { get; set; }
    }
}