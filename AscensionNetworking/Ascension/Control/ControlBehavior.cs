using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Ascension.Networking
{
    public class ControlBehavior : MonoBehaviour
    {
        Queue<ControlCommand> commands = new Queue<ControlCommand>();

        void QueueStart(ControlCommandStart start)
        {
            commands.Enqueue(start);
        }

        void QueueShutdown(ControlCommandShutdown shutdown)
        {
            commands.Enqueue(shutdown);
        }

        void Update()
        {
            if (commands.Count > 0)
            {
                var cmd = commands.Peek();

                switch (cmd.State)
                {
                    case ControlState.Pending:
                        if (--cmd.PendingFrames < 0)
                        {
                            try
                            {
                                cmd.Run();
                            }
                            catch (Exception exn)
                            {
                                Debug.LogException(exn);
                            }

                            cmd.State = ControlState.Started;
                        }
                        break;

                    case ControlState.Started:
                        if (cmd.FinishedEvent.WaitOne(0))
                        {
                            cmd.State = ControlState.Finished;
                        }
                        break;

                    case ControlState.Failed:
                        commands.Clear();
                        break;

                    case ControlState.Finished:
                        if (--cmd.FinishedFrames < 0)
                        {
                            // we are done
                            commands.Dequeue();
                            try
                            {
                                cmd.Done();
                            }
                            catch (Exception exn)
                            {
                                Debug.LogException(exn);
                            }
                        }
                        break;
                }
            }
        }
    }

}
