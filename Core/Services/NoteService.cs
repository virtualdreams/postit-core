using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using notes.Core.Models;

namespace notes.Core.Services
{
    public class NoteService
	{
		private readonly ILogger<NoteService> Log;
		private readonly MongoContext Context;

		public NoteService(ILogger<NoteService> log, MongoContext context)
		{
			Log = log;
			Context = context;
		}
		
		/// <summary>
		/// Get a list of notes.
		/// </summary>
		/// <param name="user">The user who owns the notes.</param>
		/// <param name="next">The next id.</param>
		/// <param name="trashed">Request trashed items or not.</param>
		/// <param name="limit">Limit the result.</param>
		/// <returns></returns>
		public Tuple<IEnumerable<Note>, bool> Get(ObjectId user, ObjectId next, bool trashed, int limit)
		{
			var _filter = Builders<Note>.Filter;
			var _user = _filter.Eq(f => f.Owner, user);
			var _active = _filter.Eq(f => f.Trash, trashed);
			var _next = _filter.Lt(f => f.Id, next);
			
			var _sort = Builders<Note>.Sort;
			var _order = _sort.Descending(f => f.Id);

			var _query = _user & _active;

			if(next != ObjectId.Empty)
				_query &= _next;

			Log.LogDebug($"Request notes.");

			var _result = Context.Note.Find(_query).Sort(_order).Limit(limit + 1);

			return new Tuple<IEnumerable<Note>, bool>
			(
				_result.ToEnumerable().Take(limit),
				_result.Count() > limit
			);
		}

		/// <summary>
		/// Get a list of notes by notebook name.
		/// </summary>
		/// <param name="user">The user who owns the notes.</param>
		/// <param name="notebook">The notebook name.</param>
		/// <param name="next">The next cursor.</param>
		/// <param name="limit">Limit the result.</param>
		/// <returns></returns>
		public Tuple<IEnumerable<Note>, bool> GetByNotebook(ObjectId user, string notebook, ObjectId next, int limit)
		{
			notebook = notebook?.Trim();

			if(String.IsNullOrEmpty(notebook))
			{
				return new Tuple<IEnumerable<Note>, bool>
				(
					Enumerable.Empty<Note>(),
					false
				);
			}

			var _filter = Builders<Note>.Filter;
			var _notebook = _filter.Eq(f => f.Notebook, notebook);
			var _user = _filter.Eq(f => f.Owner, user);
			var _active = _filter.Eq(f => f.Trash, false);
			var _next = _filter.Lt(f => f.Id, next);
			
			var _sort = Builders<Note>.Sort;
			var _order = _sort.Descending(f => f.Id);

			var _query = _notebook &_user & _active;

			if(next != ObjectId.Empty)
				_query &= _next;

			Log.LogDebug($"Request notes by notebook '{notebook}'.");

			var _result = Context.Note.Find(_query).Sort(_order).Limit(limit + 1);

			return new Tuple<IEnumerable<Note>, bool>
			(
				_result.ToEnumerable().Take(limit),
				_result.Count() > limit
			);
		}

		/// <summary>
		/// Get a list of notes by tag.
		/// </summary>
		/// <param name="user">The user who owns the notes.</param>
		/// <param name="tag"></param>
		/// <param name="next">The next cursor.</param>
		/// <param name="limit">Limit the result.</param>
		/// <returns></returns>
		public Tuple<IEnumerable<Note>, bool> GetByTag(ObjectId user, string tag, ObjectId next, int limit)
		{
			tag = tag?.Trim();

			if(String.IsNullOrEmpty(tag))
			{
				return new Tuple<IEnumerable<Note>, bool>
				(
					Enumerable.Empty<Note>(),
					false
				);
			}

			var _filter = Builders<Note>.Filter;
			var _tag = _filter.AnyIn("Tags", new string[] { tag });
			var _user = _filter.Eq(f => f.Owner, user);
			var _active = _filter.Eq(f => f.Trash, false);
			var _next = _filter.Lt(f => f.Id, next);
			
			var _sort = Builders<Note>.Sort;
			var _order = _sort.Descending(f => f.Id);

			var _query = _tag & _user & _active;

			if(next != ObjectId.Empty)
				_query &= _next;

			Log.LogDebug($"Request notes by tag '{tag}'.");

			var _result =  Context.Note.Find(_query).Sort(_order).Limit(limit + 1);

			return new Tuple<IEnumerable<Note>, bool>
			(
				_result.ToEnumerable().Take(limit),
				_result.Count() > limit
			);
		}

		/// <summary>
		/// Get a note by id.
		/// </summary>
		/// <param name="note">The note id.</param>
		/// <param name="user">The user id.</param>
		/// <returns></returns>
		public Note GetById(ObjectId note, ObjectId user)
		{
			var _filter = Builders<Note>.Filter;
			var _id = _filter.Eq(f => f.Id, note);
			var _user = _filter.Eq(f => f.Owner, user);

			Log.LogDebug($"Get note by id '{note.ToString()}'.");

			return Context.Note.Find(_id & _user).SingleOrDefault();
		}

		/// <summary>
		/// Get a list of notebooks.
		/// </summary>
		/// <param name="user">The users notebooks.</param>
		/// <returns>A list of notebook.</returns>
		public IEnumerable<string> Notebooks(ObjectId user)
		{
			return Context.Note.Distinct<string>("Notebook", new ExpressionFilterDefinition<Note>(f => f.Owner == user && f.Trash == false)).ToEnumerable().Where(s => !String.IsNullOrEmpty(s)).OrderBy(s => s);
		}

		/// <summary>
		/// Get a list of tags,
		/// </summary>
		/// <param name="user">The users tags.</param>
		/// <returns>A list of tags.</returns>
		public IEnumerable<string> Tags(ObjectId user)
		{
			return Context.Note.Distinct<string>("Tags", new ExpressionFilterDefinition<Note>(f => f.Owner == user && f.Trash == false)).ToEnumerable().Where(s => !String.IsNullOrEmpty(s)).OrderBy(s => s);
		}

		/// <summary>
		/// Create a new note.
		/// </summary>
		/// <param name="user">The user who owns thos note.</param>
		/// <param name="title">The note title.</param>
		/// <param name="content">The note content.</param>
		/// <returns></returns>
		public ObjectId Create(ObjectId user, string title, string content, string notebook, string tags)
		{
			var _tags = tags?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToArray();

			var _note = new Note
			{
				Owner = user,
				Title = title?.Trim(),
				Content = content?.Trim(),
				Tags = _tags,
				Notebook = notebook?.Trim(),
				Trash = false,
				Version = 1
			};

			Context.Note.InsertOne(_note);

			Log.LogDebug($"Create new note with id '{_note.Id.ToString()}'.");

			return _note.Id;
		}

		/// <summary>
		/// Update a note.
		/// </summary>
		/// <param name="note">The note.</param>
		/// <param name="title">The new title.</param>
		/// <param name="content">The new content.</param>
		/// <returns></returns>
		public void Update(ObjectId note, ObjectId user, string title, string content, string notebook, string tags)
		{
			var _tags = tags?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToArray();

			var _filter = Builders<Note>.Filter;
			var _id = _filter.Eq(f => f.Id, note);
			var _user = _filter.Eq(f => f.Owner, user);

			var _update = Builders<Note>.Update;
			var _set = _update
						.Set(f => f.Title, title?.Trim())
						.Set(f => f.Content, content?.Trim())
						.Set(f => f.Notebook, notebook?.Trim())
						.Set(f => f.Tags, _tags)
						.Inc(f => f.Version, 1);

			Log.LogInformation($"Update note '{note.ToString()}'.");

			Context.Note.UpdateOne(_id, _set, new UpdateOptions { IsUpsert = true });
		}

		/// <summary>
		/// Toogle trash status flag.
		/// </summary>
		/// <param name="note"></param>
		/// <param name="user"></param>
		public void Trash(ObjectId note, bool trash)
		{
			var _filter = Builders<Note>.Filter;
			var _id = _filter.Eq(f => f.Id, note);

			var _update = Builders<Note>.Update;
			var _set = _update
				.Set(f => f.Trash, trash);

			Log.LogDebug($"Mark note '{note.ToString()}' as trash/restore ({trash}).");

			Context.Note.UpdateOne(_id, _set, new UpdateOptions { IsUpsert = true });
		}

		/// <summary>
		/// Remove the note.
		/// </summary>
		/// <param name="note">The note id.</param>
		/// <param name="user">The user who own this note.</param>
		public void Delete(ObjectId note, ObjectId user)
		{
			var _filter = Builders<Note>.Filter;
			var _id = _filter.Eq(f => f.Id, note);
			var _user = _filter.Eq(f => f.Owner, user);

			Log.LogDebug($"Delete note '{note.ToString()}' permanently.");

			Context.Note.DeleteOne(_id & _user);
		}

		/// <summary>
		/// Search for a term.
		/// </summary>
		/// <param name="user">The user who owns the notes.</param>
		/// <param name="term">The term to search for.</param>
		/// <param name="previous">The previous cursor.</param>
		/// <param name="next">The next cursor.</param>
		/// <param name="limit">Limit the result.</param>
		/// <returns></returns>
		public Tuple<IEnumerable<Note>, bool> Search(ObjectId user, string term, ObjectId next, int limit)
		{
			term = term?.Trim();

			if(String.IsNullOrEmpty(term))
			{
				return new Tuple<IEnumerable<Note>, bool>
				(
					Enumerable.Empty<Note>(),
					false
				);
			}

			var _filter = Builders<Note>.Filter;
			var _user = _filter.Eq(f => f.Owner, user);
			var _text = _filter.Text(term);
			var _active = _filter.Eq(f => f.Trash, false);

			var _projection = Builders<Note>.Projection;
			var _score = _projection.MetaTextScore("Score");

			var _sort = Builders<Note>.Sort;
			var _order = _sort.MetaTextScore("Score");
			
			var _query = _user & _text & _active;

			Log.LogDebug($"Search for notes with term '{term}'.");

			var _result = Context.Note.Find(_query).Project<Note>(_score).Sort(_order).Limit(100);
			if(next != ObjectId.Empty)
			{
				var _skip = _result.ToEnumerable().SkipWhile(condition => condition.Id != next).Skip(1);

				return new Tuple<IEnumerable<Note>, bool>
				(
					_skip.Take(limit),
					_skip.Count() > limit
				);
			}

			return new Tuple<IEnumerable<Note>, bool>
			(
				_result.ToEnumerable().Take(limit),
				_result.Count() > limit
			);
		}
	}
}