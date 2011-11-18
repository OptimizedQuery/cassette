﻿#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion


using Cassette.Configuration;
namespace Cassette
{
    // Subclass so we can test the non-abstract implementation.
    public class TestableBundle : Bundle
    {
        public TestableBundle(string path) : base(path)
        {
        }

        public bool WasProcessed { get; set; }

        public bool WasDisposed { get; set; }

        public string RenderResult { get; set; }

        internal override string Render()
        {
            return (RenderResult);
        }

        internal override void Process(CassetteSettings settings)
        {
            WasProcessed = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WasDisposed = true;
        }
    }
}
