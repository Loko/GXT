using System;

namespace GXT_SANDBOX
{
#if WINDOWS || XBOX
    static class Program
    {
        // COLLECTION OF KNOWN TEST CASES
        // SET TEST TO ONE OF THEM AND RECOMPILE TO TRY THEM OUT
        public static readonly string ASSERT_TEST = "assert_test";  // this test is intended to fail, assertions in GXT are like those in c/c++
        public static readonly string ROOT_TEST = "root_test";
        public static readonly string ANIMATION_TEST = "animation_test";
        public static readonly string PROCESS_TEST = "process_test";
        public static readonly string COLLISION_TEST = "collision_test";    // comment out #undef DEBUG_DRAW_SIMPLEX IN gxtGJKCollider to see simplex
        public static readonly string KINEMATIC_TEST = "kinematic_test";
        public static readonly string RAY_TEST = "ray_test";
        //public static readonly string TEXTURE_MAPPING_TEST = "texture_mapping_test";
        public static readonly string TEXTURE_MAPPING_TEST2 = "texture_mapping_test2";
        public static readonly string PHYSICS_STRESS_TEST = "physics_stress_test";
        public static readonly string CONFIG_TEST = "config_test";
        public static readonly string SAP_TEST = "sap_test";
        public static readonly string AUDIO_TEST = "audio_test";
        public static readonly string SCENE_GRAPH_TEST = "scene_graph_test";
        public static readonly string CUSTOM_ANIMATION_TEST = "custom_animation_test";
        public static readonly string CIRCULAR_BUFFER_TEST = "circular_buffer_test";
        public static readonly string KEYBOARD_LISTENER_TEST = "keyboard_listener_test";
        public static readonly string SPRITEBATCH_TEST = "spritebatch_test";
        public static readonly string HASHEDSTRING_TEST = "hashedstring_test";
        public static readonly string ASG_PROTOTYPE_TEST = "asg_prototype_test";
        public static string TEST = KEYBOARD_LISTENER_TEST;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (TEST == ASSERT_TEST)
                Assert_Test.RunTest();
            else if (TEST == ROOT_TEST)
                Root_Test.RunTest();
            else if (TEST == ANIMATION_TEST)
                Animation_Test.RunTest();
            else if (TEST == PROCESS_TEST)
                ProcessTest.RunTest();
            else if (TEST == COLLISION_TEST)
                CollisionTest.RunTest();
            else if (TEST == KINEMATIC_TEST)
                KinematicTest.RunTest();
            else if (TEST == RAY_TEST)
                RayTest.RunTest();
            //else if (TEST == TEXTURE_MAPPING_TEST)
            //    TextureMappingTest.RunTest();
            else if (TEST == PHYSICS_STRESS_TEST)
                PhysicsStressTest.RunTest();
            else if (TEST == CONFIG_TEST)
                ConfigTest.RuntTest();
            else if (TEST == SAP_TEST)
                SAPTest.RunTest();
            else if (TEST == AUDIO_TEST)
                AudioTest.RunTest();
            //else if (TEST == TEXTURE_MAPPING_TEST2)
            //   TextureMappingTest2.RunTest();
            else if (TEST == SCENE_GRAPH_TEST)
                SceneGraphTest.RunTest();
            else if (TEST == HASHEDSTRING_TEST)
                HashedStringTest.RunTest();
            else if (TEST == CUSTOM_ANIMATION_TEST)
                MyAnimationTest.RunTest();
            else if (TEST == CIRCULAR_BUFFER_TEST)
                CircularBufferTest.RunTest();
            else if (TEST == KEYBOARD_LISTENER_TEST)
                KeyboardListenerTest.RunTest();
            else if (TEST == SPRITEBATCH_TEST)
                SpriteBatchTest.RunTest();
            else if (TEST == ASG_PROTOTYPE_TEST)
                ASGPrototype.RunTest();
            else
                Console.WriteLine("The Test Case: {0} Was Not Found!", TEST);
        }
    }
#endif
}

