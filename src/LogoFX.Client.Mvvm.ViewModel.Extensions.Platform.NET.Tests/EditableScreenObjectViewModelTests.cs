﻿using System.Linq;
using Caliburn.Micro;
using FluentAssertions;
using LogoFX.Client.Mvvm.ViewModel.Services;
using LogoFX.Client.Mvvm.ViewModel.Shared;
using LogoFX.Client.Testing.Integration.xUnit;
using LogoFX.Client.Testing.Shared.Caliburn.Micro;
using Xunit;

namespace LogoFX.Client.Mvvm.ViewModel.Extensions.Tests
{    
    public class EditableScreenObjectViewModelTests : IntegrationTestsBase<TestConductorViewModel, TestBootstrapper>
    {        
        //Note: may use here IntegrationTestsBaseWithActivation as well - package still not available.
        protected override TestConductorViewModel CreateRootObjectOverride(TestConductorViewModel rootObject)
        {
            ScreenExtensions.TryActivate(rootObject);
            return rootObject;
        }

        protected override void OnAfterTeardown()
        {
            base.OnAfterTeardown();
            TestHelper.Teardown();
        }        

        [Fact]        
        public void ModelIsChanged_WhenViewModelIsClosed_MessageBoxIsDisplayed()
        {
            var simpleModel = new SimpleEditableModel();
            var mockMessageService = new FakeMessageService();

            var rootObject = CreateRootObject();
            var screenObjectViewModel = new TestEditableScreenSimpleObjectViewModel(mockMessageService, simpleModel);
            rootObject.ActivateItem(screenObjectViewModel);            
            simpleModel.Name = DataGenerator.ValidName;
            screenObjectViewModel.CloseCommand.Execute(null);

            var wasCalled = mockMessageService.WasCalled;
            wasCalled.Should().BeTrue();            
        }

        [Fact]        
        public void ModelIsNotChanged_WhenViewModelIsClosed_MessageBoxIsNotDisplayed()
        {
            var simpleModel = new SimpleEditableModel();
            var mockMessageService = new FakeMessageService();

            var rootObject = CreateRootObject();
            var screenObjectViewModel = new TestEditableScreenSimpleObjectViewModel(mockMessageService, simpleModel);
            rootObject.ActivateItem(screenObjectViewModel);            
            screenObjectViewModel.CloseCommand.Execute(null);

            var wasCalled = mockMessageService.WasCalled;
            wasCalled.Should().BeFalse();
        }

        [Fact]        
        public void ModelIsChanged_WhenViewModelIsClosedAndMessageResultIsYes_ModelIsSaved()
        {
            var simpleModel = new SimpleEditableModel();
            var mockMessageService = new FakeMessageService();
            mockMessageService.SetMessageResult(MessageResult.Yes);

            var rootObject = CreateRootObject();
            var screenObjectViewModel = new TestEditableScreenSimpleObjectViewModel(mockMessageService, simpleModel);
            rootObject.ActivateItem(screenObjectViewModel);
            const string expectedValue = DataGenerator.ValidName;
            simpleModel.Name = expectedValue;
            screenObjectViewModel.CloseCommand.Execute(null);

            var isDirty = simpleModel.IsDirty;
            isDirty.Should().BeFalse();
            var actualValue = simpleModel.Name;
            actualValue.Should().Be(expectedValue);
        }

        [Fact]        
        public void ModelIsChanged_WhenViewModelIsClosedAndMessageResultIsNo_ModelIsNotSaved()
        {
            string initialValue = string.Empty;
            var simpleModel = new SimpleEditableModel(initialValue,20);
            var mockMessageService = new FakeMessageService();
            mockMessageService.SetMessageResult(MessageResult.No);
            RegisterInstance<IMessageService>(mockMessageService);

            var rootObject = CreateRootObject();         
            var screenObjectViewModel = new TestEditableScreenSimpleObjectViewModel(mockMessageService, simpleModel);
            rootObject.ActivateItem(screenObjectViewModel);                        
            simpleModel.Name = DataGenerator.ValidName;
            screenObjectViewModel.CloseCommand.Execute(null);

            var isDirty = simpleModel.IsDirty;
            isDirty.Should().BeFalse();
            var actualValue = simpleModel.Name;
            actualValue.Should().Be(initialValue);
        }

        [Fact]        
        public void ModelIsChanged_WhenViewModelIsClosedAndMessageResultIsCancel_ModelIsSaved()
        {
            string initialValue = string.Empty;
            const string newValue = DataGenerator.ValidName;
            var simpleModel = new SimpleEditableModel(initialValue, 20);
            var mockMessageService = new FakeMessageService();
            mockMessageService.SetMessageResult(MessageResult.Cancel);
           
            var rootObject = CreateRootObject();
            var screenObjectViewModel = new TestEditableScreenSimpleObjectViewModel(mockMessageService, simpleModel);
            rootObject.ActivateItem(screenObjectViewModel);
            simpleModel.Name = newValue;
            screenObjectViewModel.CloseCommand.Execute(null);            

            var isDirty = simpleModel.IsDirty;
            isDirty.Should().BeTrue();
            var actualValue = simpleModel.Name;
            actualValue.Should().Be(DataGenerator.ValidName);            
        }

        [Fact]        
        public void ModelIsChanged_WhenViewModelIsClosedAndMessageResultIsNo_ThenOnChangesCancelingIsCalled()
        {
            string initialValue = string.Empty;
            var simpleModel = new SimpleEditableModel(initialValue, 20);
            var mockMessageService = new FakeMessageService();
            mockMessageService.SetMessageResult(MessageResult.No);
            RegisterInstance<IMessageService>(mockMessageService);

            var rootObject = CreateRootObject();
            var screenObjectViewModel = new TestEditableScreenSimpleObjectViewModel(mockMessageService, simpleModel);
            rootObject.ActivateItem(screenObjectViewModel);
            simpleModel.Name = DataGenerator.ValidName;
            screenObjectViewModel.CloseCommand.Execute(null);

            var wasCalled = screenObjectViewModel.WasCancelingChangesCalled;
            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void WhenModelIsChangedAndChangesAreAppliedAndModelIsChangedAndChangesAreCancelled_ThenCorrectModelIsDisplayed()
        {
            var initialPhones = new[] { 546, 432 };
            var compositeModel = new CompositeEditableModel("Here", initialPhones);
            var mockMessageService = new FakeMessageService();            
            
            var rootObject = CreateRootObject();
            var screenObjectViewModel = new TestEditableScreenCompositeObjectViewModel(mockMessageService, compositeModel);
            rootObject.ActivateItem(screenObjectViewModel);
            compositeModel.AddPhone(647);
            screenObjectViewModel.ApplyCommand.Execute(null);
            compositeModel.AddPhone(555);
            screenObjectViewModel.CancelChangesCommand.Execute(null);

            var phones = ((ICompositeEditableModel)compositeModel).Phones.ToArray();
            var expectedPhones = new[] { 546, 432, 647 };
            phones.ShouldBeEquivalentTo(expectedPhones);            
        }
    }
}