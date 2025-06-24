using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Constants shared across gameplay systems.
    /// Ported from common/src/consts.rs
    /// </summary>
    public static class Consts {
        public const float MAX_PICKUP_RANGE = 5.0f;
        public const float MAX_MOUNT_RANGE = 5.0f;
        public const float MAX_SPRITE_MOUNT_RANGE = 2.0f;
        public const float MAX_TRADE_RANGE = 5.0f;
        public const float MAX_NPCINTERACT_RANGE = 8.0f;
        public const float MAX_INTERACT_RANGE = 5.0f;
        public const float MAX_WAYPOINT_RANGE = 4.0f;
        public const float MAX_CAMPFIRE_RANGE = MAX_WAYPOINT_RANGE - 0.001f;

        public const float GRAVITY = 25.0f;
        public const float FRIC_GROUND = 0.15f;

        public const float AIR_DENSITY = 1.225f;
        public const float WATER_DENSITY = 999.1026f;
        public const float LAVA_DENSITY = 3000.0f;
        public const float IRON_DENSITY = 7870.0f;
        public const float HUMAN_DENSITY = 990.0f;

        public const int MIN_RECOMMENDED_RAYON_THREADS = 2;
        public const int MIN_RECOMMENDED_TOKIO_THREADS = 2;

        public const float SOUND_TRAVEL_DIST_PER_VOLUME = 3.0f;

        public const float TELEPORTER_RADIUS = 3.0f;

        public const double DAY_LENGTH_DEFAULT = 30.0;
    }
}

