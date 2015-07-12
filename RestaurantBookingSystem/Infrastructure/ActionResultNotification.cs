using System;

namespace RestaurantBookingSystem.Infrastructure
{
    [Serializable]
    public class ActionResultNotification
    {
        /// <summary>
        /// Gets or Sets wheather the result was true or false
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Gets or Sets the State of the Notification Message
        /// </summary>
        public MessageState State { get; set; }
        
        /// <summary>
        /// Gets or Sets the Message string
        /// </summary>
        public string Message { get; set; }

        public enum MessageState
        {
            /// <summary>
            /// There is no message or message dipicts nothing
            /// </summary>
            None,

            /// <summary>
            /// The Message is an Information Discription
            /// </summary>
            Information,

            /// <summary>
            /// The Message is a Warning Discription
            /// </summary>
            Warning,

            /// <summary>
            /// The Message is a Fatal Error Discription
            /// </summary>
            Error
        }
    }
}