using System;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace RetoFinal
{
    public class SCT_Tic_MtoAdapter : BaseAdapter<SCT_Tic_Mto>
    {
        Activity activity;
        int layoutResourceId;
        List<SCT_Tic_Mto> items = new List<SCT_Tic_Mto>();

        public SCT_Tic_MtoAdapter(Activity activity, int layoutResourceId)
        {
            this.activity = activity;
            this.layoutResourceId = layoutResourceId;
        }

        //Returns the view for a specific item on the list
        public override View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            var row = convertView;
            var currentItem = this[position];
            CheckBox checkBox;

            if (row == null)
            {
                var inflater = activity.LayoutInflater;
                row = inflater.Inflate(layoutResourceId, parent, false);

                checkBox = row.FindViewById<CheckBox>(Resource.Id.checkToDoItem);

                checkBox.CheckedChange += async (sender, e) =>
                {
                    var cbSender = sender as CheckBox;
                    if (cbSender != null && cbSender.Tag is SCT_Tic_MtoWrapper && cbSender.Checked)
                    {
                        cbSender.Enabled = false;
                        if (activity is TicketsActivity)
                            await ((TicketsActivity)activity).CheckItem((cbSender.Tag as SCT_Tic_MtoWrapper).SCT_Tic_Mto);
                    }
                };
            }
            else
                checkBox = row.FindViewById<CheckBox>(Resource.Id.checkToDoItem);

            checkBox.Text = currentItem.Asunto;
            checkBox.Checked = false;
            checkBox.Enabled = true;
            checkBox.Tag = new SCT_Tic_MtoWrapper(currentItem);

            return row;
        }

        public void Add(SCT_Tic_Mto item)
        {
            items.Add(item);
            NotifyDataSetChanged();
        }

        public void Clear()
        {
            items.Clear();
            NotifyDataSetChanged();
        }

        public void Remove(SCT_Tic_Mto item)
        {
            items.Remove(item);
            NotifyDataSetChanged();
        }

        #region implemented abstract members of BaseAdapter

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get
            {
                return items.Count;
            }
        }

        public override SCT_Tic_Mto this[int position]
        {
            get
            {
                return items[position];
            }
        }

        #endregion
    }
}

