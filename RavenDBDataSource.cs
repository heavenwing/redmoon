using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using ITKE.WinLib;
using Raven.Client;
using Raven.Client.Linq;

namespace eBalance_IE_Extensions.EntryDev
{
    public class RavenDBDataSource<T>
    {
        private BindingNavigator _bn;
        private BindingSource _bs;
        private IDocumentStore _store;
        private DelayEvent delayEvent;

        public static List<T> LoadAll(IDocumentSession session, int pageSize)
        {
            var all = new List<T>();
            var query = session.Query<T>();
            var total = query.Count();
            var page = total / pageSize;
            if ((total % pageSize) > 0) page++;
            for (int i = 0; i < page; i++)
            {
                var lst = query.Skip(i * pageSize).Take(pageSize).ToList();
                all.AddRange(lst);
            }
            return all;
        }

        public RavenDBDataSource(IDocumentStore store, BindingNavigator bn, BindingSource bs)
        {
            _store = store;
            this.PageSize = 20;

            _bn = bn;
            _bs = bs;

            if (_bn.AddNewItem != null)
                _bn.AddNewItem.Visible = false;
            if (_bn.DeleteItem != null)
                _bn.DeleteItem.Visible = false;

            _bn.MoveFirstItem.Click += MoveFirstItem_Click;
            _bn.MoveLastItem.Click += MoveLastItem_Click;
            _bn.MoveNextItem.Click += MoveNextItem_Click;
            _bn.MovePreviousItem.Click += MovePreviousItem_Click;

            _bn.MoveFirstItem.Enabled = true;
            _bn.MoveLastItem.Enabled = true;
            _bn.MoveNextItem.Enabled = true;
            _bn.MovePreviousItem.Enabled = true;
            _bn.PositionItem.Enabled = true;

            delayEvent = new DelayEvent();
            delayEvent.ActualDo += delayEvent_ActualDo;
        }

        void delayEvent_ActualDo()
        {
            int position;
            if (int.TryParse(_bn.PositionItem.Text, out position))
            {
                _currentPage = position;

                DoQuery();
            }
            else
            {
                _bn.PositionItem.Text = _currentPage.ToString();
            }
        }

        void PositionItem_TextChanged(object sender, EventArgs e)
        {
            delayEvent.Do();
        }

        public int PageSize { get; set; }

        private int _pageCount;
        private int _currentPage;

        public List<T> DataSource { get; private set; }

        public void Load()
        {
            _indexName = null;
            _criteria = null;

            _currentPage = 1;

            DoQuery();
        }

        private string _indexName;
        private Func<IRavenQueryable<T>, IRavenQueryable<T>> _criteria;
        public void Load(Func<IRavenQueryable<T>, IRavenQueryable<T>> criteria, string indexName = "")
        {
            _criteria = criteria;
            _indexName = indexName;

            _currentPage = 1;

            DoQuery();
        }

        private void DoQuery()
        {
            using (var session = _store.OpenSession())
            {
                var query = string.IsNullOrEmpty(_indexName)
                    ? session.Query<T>()
                    : session.Query<T>(_indexName);
                if (_criteria != null)
                    query = _criteria(query);

                var count = query.Count();
                _pageCount = count / PageSize;

                if ((count % PageSize) > 0)
                    _pageCount++;

                _bn.CountItem.Text = string.Format("/{0}", _pageCount);
                _bn.PositionItem.TextChanged -= PositionItem_TextChanged;
                _bn.PositionItem.Text = _currentPage.ToString();
                _bn.PositionItem.TextChanged += PositionItem_TextChanged;

                this.DataSource = query
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                _bs.DataSource = this.DataSource;
                _bs.ResetBindings(false);
            }
        }

        void MoveLastItem_Click(object sender, EventArgs e)
        {
            if (_currentPage < _pageCount)
            {
                _currentPage = _pageCount;
                DoQuery();
            }
        }

        void MoveNextItem_Click(object sender, EventArgs e)
        {
            if (_currentPage < _pageCount)
            {
                _currentPage++;
                DoQuery();
            }
        }

        void MovePreviousItem_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                DoQuery();
            }
        }

        void MoveFirstItem_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage = 1;
                DoQuery();
            }
        }
    }
}
