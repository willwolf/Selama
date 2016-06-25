﻿using Microsoft.AspNet.Identity;
using Selama.Areas.Forums.Models;
using Selama.Areas.Forums.ViewModels;
using Selama.Classes.Enum;
using Selama.Classes.Utility;
using Selama.Controllers;
using Selama.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Selama.Areas.Forums.Controllers
{
    public class ForumController : _BaseAuthorizeController
    {
        public ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Forums/Forum
        public ActionResult Index()
        {
            List<ForumSectionViewModel> forums = Util.ConvertLists<ForumSection, ForumSectionViewModel>(
                _db.ForumSections.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder),
                section => new ForumSectionViewModel(section)
            );

            return View(forums);
        }

        public ActionResult Threads(int id = 0)
        {
            Forum forum = _db.Forums.Find(id);
            if (forum == null)
            {
                return RedirectToAction("Index");
            }

            return View(new ForumViewModel(forum));
        }

        public ActionResult Thread(int id = 0)
        {
            Thread thread = _db.Threads.Find(id);
            if (thread == null)
            {
                return RedirectToAction("Index");
            }

            return View(new ThreadViewModel(thread));
        }

        #region Create thread
        public ActionResult CreateThread(int id = 0)
        {
            Forum forum = _db.Forums.Where(f => f.IsActive && f.ID == id).FirstOrDefault();
            if (forum == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateThread(ThreadViewModel thread, int id = 0)
        {
            Forum forum = _db.Forums.Where(f => f.IsActive && f.ID == id).FirstOrDefault();
            if (forum == null)
            {
                return RedirectToAction("Index");
            }

            thread.PostDate = DateTime.Now;
            thread.ValidateModel(ModelState);
            if (ModelState.IsValid)
            {
                _db.Threads.Add(new Thread(thread, User.Identity.GetUserId(), id));
                SaveChangeError result;
                if (TrySaveChanges(_db, out result))
                {
                    return RedirectToAction("Thread", new { id = id });
                }
            }

            return View(thread);
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostReply(ThreadReplyViewModel reply, int id = 0)
        {
            Thread thread = _db.Threads.Find(id);
            if (thread == null || !thread.IsActive)
            {
                return HttpNotFound();
            }

            if (id != reply.ThreadID)
            {
                ModelState.AddModelError("ThreadID", "Invalid thread selected");
            }
            reply.PostDate = DateTime.Now;
            reply.ValidateModel(ModelState);
            if (ModelState.IsValid)
            {
                ThreadReply dbReply = new ThreadReply(reply, User.Identity.GetUserId(), id);
                _db.ThreadReplies.Add(dbReply);
                if (TrySaveChanges(_db))
                {
                    _db = new ApplicationDbContext();
                    dbReply = _db.ThreadReplies.Find(dbReply.ID);
                    Response.StatusCode = 200;

                    return PartialView("DisplayTemplates/ThreadReplyViewModel", new ThreadReplyViewModel(dbReply));
                }
            }

            // Generate the errors as a seriarlizable list of objects
            List<object> errors = new List<object>();
            foreach (var error in ModelState)
            {
                if (error.Value.Errors.Count > 0)
                {
                    var errorObj = new
                    {
                        Property = error.Key,
                        Errors = new List<string>()
                    };
                    foreach (var errorMsg in error.Value.Errors)
                    {
                        errorObj.Errors.Add(errorMsg.ErrorMessage);
                    }
                    errors.Add(errorObj);
                }
            }

            Response.StatusCode = 400; // Bad Request
            return Json(errors);
        }

        public ActionResult EditThread(int id = 0)
        {
            Thread thread = _db.Threads.Find(id);
            if (thread == null || !thread.IsActive)
            {
                return HttpNotFound("Invalid ID");
            }

            return PartialView("EditorTemplates/ThreadEditViewModel", new ThreadEditViewModel(thread));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditThread(ThreadEditViewModel thread)
        {
            Thread dbThread = _db.Threads.Find(thread.ID);
            if (dbThread == null || !dbThread.IsActive)
            {
                return HttpNotFound("Invalid ID");
            }

            thread.ValidateModel(ModelState);
            if (dbThread.AuthorID != User.Identity.GetUserId())
            {
                ModelState.AddModelError("", "You are not the author of this post");
            }
            if (ModelState.IsValid)
            {
                dbThread.UpdateFromViewModel(thread);
                if (TrySaveChanges(_db))
                {
                    _db.Entry(dbThread).Reload();
                    return Json(new ThreadViewModel(dbThread).HtmlContent.ToString());
                }
            }

            return HttpUnprocessable();
        }

        public ActionResult EditReply(int id = 0)
        {
            ThreadReply reply = _db.ThreadReplies.Find(id);
            if (reply == null || !reply.IsActive)
            {
                return HttpNotFound("Invalid ID");
            }

            return PartialView("EditorTemplates/ThreadReplyEditViewModel", new ThreadReplyEditViewModel(reply));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditReply(ThreadReplyEditViewModel reply)
        {
            ThreadReply dbReply = _db.ThreadReplies.Find(reply.ID);
            if (reply == null || !dbReply.IsActive)
            {
                return HttpNotFound("Invalid ID");
            }

            reply.ValidateModel(ModelState);
            if (dbReply.ThreadID != reply.ThreadID)
            {
                ModelState.AddModelError("ThreadID", "Invalid Thread ID");
            }
            if (dbReply.AuthorID != User.Identity.GetUserId())
            {
                ModelState.AddModelError("", "You are not the author of this post, and therfore cannot edit it");
            }
            if (ModelState.IsValid)
            {
                dbReply.UpdateFromViewModel(reply);
                if (TrySaveChanges(_db))
                {
                    _db.Entry(dbReply).Reload();
                    return Json(new { id = dbReply.ID, content = new ThreadReplyViewModel(dbReply).HtmlContent.ToString() });
                }
            }

            return HttpUnprocessable();
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}