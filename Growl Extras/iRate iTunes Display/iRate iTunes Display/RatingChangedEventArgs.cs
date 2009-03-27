using System;
using System.Collections.Generic;
using System.Text;

namespace iRate.Controls
{
    public class RatingChangedEventArgs : EventArgs
    {
        private int _oldRating;
        public int OldRating
        {
            get { return _oldRating; }
        }

        private int _newRating;
        public int NewRating
        {
            get { return _newRating; }
        }

        public RatingChangedEventArgs(int oldValue, int newValue)
        {
            _oldRating = oldValue;
            _newRating = newValue;
        }

    }

}
