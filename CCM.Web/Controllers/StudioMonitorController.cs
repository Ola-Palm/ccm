/*
 * Copyright (c) 2017 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using CCM.Core.Entities;
using CCM.Core.Interfaces.Repositories;
using CCM.Web.Models.Studio;
using CCM.Web.Models.StudioMonitor;

namespace CCM.Web.Controllers
{
    // Monitorering av studio
    public class StudioMonitorController : Controller
    {
        private readonly IStudioRepository _studioRepository;

        public StudioMonitorController(IStudioRepository studioRepository)
        {
            _studioRepository = studioRepository;
        }

        public ActionResult Index(Guid id)
        {
            var studio = _studioRepository.GetById(id);
            var vm = Mapper.Map<StudioMonitorViewModel>(studio);

            // Url ingår bara om kameran är aktiv.
            if (studio.CameraActive)
            {
                var baseUrl = new UriBuilder
                {
                    Scheme = "http",
                    Host = studio.CameraAddress,
                    UserName = studio.CameraUsername,
                    Password = !string.IsNullOrEmpty(studio.CameraUsername) ? studio.CameraPassword : string.Empty // Lösenord utan användarnamn ger ogiltig url.
                }.Uri;

                vm.CameraVideoUrl = new Uri(baseUrl, studio.CameraVideoUrl).ToString();
                vm.CameraImageUrl = new Uri(baseUrl, studio.CameraImageUrl).ToString();
                vm.CameraPlayAudioUrl = new Uri(baseUrl, studio.CameraPlayAudioUrl).ToString();
            }
            else
            {
                vm.CameraVideoUrl = vm.CameraImageUrl = string.Empty;
            }

            // TODO: Lägg till egenskaper från studions kodare + ev kodarstatus
            // TODO: T.ex. kan max antal ljudingångar och GPO:er kollas här.

            return View(vm);
        }

        [ChildActionOnly]
        public ActionResult Menu()
        {
            IList<Studio> studios;
            try
            {
                studios = _studioRepository.GetAll().OrderBy(s => s.Name).ToList();
            }
            catch (Exception)
            {
                studios = new List<Studio>();
            } 
            return View(studios);
        }
    }
}