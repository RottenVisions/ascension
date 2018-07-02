using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ascension.Networking {
  public delegate void PropertyCallback(IState state, string propertyPath, ArrayIndices arrayIndices);
  public delegate void PropertyCallbackSimple();
}
