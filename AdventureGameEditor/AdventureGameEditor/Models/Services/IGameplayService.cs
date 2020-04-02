using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public interface IGameplayService
    {
        public GameplayViewModel GetGameplayViewModel(String userName, String gameTitle);
    }
}
