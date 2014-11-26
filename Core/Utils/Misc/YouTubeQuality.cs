﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils.Misc {
    public enum YouTubeQuality : short {
        // video
        Quality144P,
        Quality240P,
        Quality270P,
        Quality360P,
        Quality480P,
        Quality520P,
        Quality720P,
        Quality1080P,
        Quality2160P,

        // audio
        QualityLow,
        QualityMedium,
        QualityHigh,

        NotAvailable,
        Unknown,
    }

    public enum YouTubeThumbnailSize : short {
        Small,
        Medium,
        Large
    }

}
