var StringHelper = require('../../../jsbin/Modules/helper').StringHelper;
var TagHelper = require('../../../jsbin/Modules/helper').TagHelper;
var expect = require('chai').expect;

/**
 * only dom irrelevant functions are tested
 */


/**
 * unit test for StringHelper class
 */
describe('StringHelper test', function(){
    
    //unit test for StringHelper.trim() method
    it(`case: StringHelper.trim()`, function(){

        var trim_case_correct = '  http://wwww.g o o g l e.com   ';
        var trim_case_space = '   ';
        var trim_case_null = null;
        var trim_case_undefined = undefined;

        expect(StringHelper.trim(trim_case_correct)).to.be.equal('http://wwww.g o o g l e.com');
        expect(StringHelper.trim(trim_case_space)).to.not.ok;
        expect(StringHelper.trim(trim_case_null)).to.be.null;
        expect(StringHelper.trim(trim_case_undefined)).to.be.undefined;
    });

    //unit test for StringHelper.cmdFormat() method
    it('case: StringHelper.cmdFormat()', function(){

        var cmdFormat_case_template_correct = '<MeetingCommand OperationType="%0" SipUri="%1" Id="%2">' +
                                            '<ResultCode>%3</ResultCode>' +
                                            '<Classification>%4</Classification>' +
                                        '</MeetingCommand>';
        var cmdFormat_case_template_null = null;
        var cmdFormat_case_template_undefined = undefined;

        var sOperationType_empty = '';
        var sSipUri_null = null;
        var sClassification_undefined = undefined;
        
        var sOperationType = 'Search';
        var sSipUri = 'sip:john.tyler@lync.nextlabs.solutions';
        var sId = '15f031d9-c40a-4fd6-a0de-8e0c46ac493f';
        var sResultCode = '0';
        var sClassification = '<Test></Test>';

        var exp_cmdFormat_case_template_correct = `<MeetingCommand OperationType="${sOperationType}" SipUri="${sSipUri}" Id="${sId}">` +
                                    `<ResultCode>${sResultCode}</ResultCode>` +
                                    `<Classification>${sClassification}</Classification>` +
                                '</MeetingCommand>';

        var exp_cmdFormat_case_arg_incorrect = `<MeetingCommand OperationType="${sOperationType_empty}" SipUri="${sSipUri_null}" Id="${sId}">` +
                            `<ResultCode>${sResultCode}</ResultCode>` +
                            `<Classification>${sClassification_undefined}</Classification>` +
                        '</MeetingCommand>';
        
        //correct template and arguments
        expect(StringHelper.cmdFormat(exp_cmdFormat_case_template_correct, sOperationType, sSipUri, sId, sResultCode, sClassification)).to.be.equal(exp_cmdFormat_case_template_correct);

        //correct template and incorrect arguments
        expect(StringHelper.cmdFormat(cmdFormat_case_template_correct, sOperationType_empty, sSipUri_null, sId, sResultCode, sClassification_undefined)).to.be.equal(exp_cmdFormat_case_arg_incorrect);
       
        //incorrect template and correct arguments
        expect(StringHelper.cmdFormat(cmdFormat_case_template_null, sOperationType, sSipUri, sId, sResultCode, sClassification)).to.be.null;
        expect(StringHelper.cmdFormat(cmdFormat_case_template_undefined, sOperationType, sSipUri, sId, sResultCode, sClassification)).to.be.undefined;

        //incorrect template and arguments
        expect(StringHelper.cmdFormat(cmdFormat_case_template_null, sOperationType_empty, sSipUri_null, sId, sResultCode, sClassification_undefined)).to.be.null;
        expect(StringHelper.cmdFormat(cmdFormat_case_template_undefined, sOperationType_empty, sSipUri_null, sId, sResultCode, sClassification_undefined)).to.be.undefined;

    });
});

/**
 * unit test for TagHelper class
 */
describe('TagHelper test', function(){
    
    //unit test for TagHelper.checkTagNodeAttrs() method
    it('case: checkXmlAttrs()', function(){

        //correct root node case
        var tagAttr_case_root_correct = {};
        tagAttr_case_root_correct[TagHelper.NODE_NAME] = TagHelper.CLASSIFICATION_NODE_NAME;
        tagAttr_case_root_correct[TagHelper.TYPE_ATTR_NAME] = TagHelper.MANUAL_TYPE;

        //correct tag node case
        var tagAttr_case_child_correct = {};
        tagAttr_case_child_correct[TagHelper.NODE_NAME] = TagHelper.TAG_NODE_NAME;
        tagAttr_case_child_correct[TagHelper.NAME_ATTR_NAME] = 'itar';
        tagAttr_case_child_correct[TagHelper.VALUES_ATTR_NAME] = 'yes|no';
        tagAttr_case_child_correct[TagHelper.DEFAULT_ATTR_NAME] = 'yes';
        tagAttr_case_child_correct[TagHelper.EDITABLE_ATTR_NAME] = 'true';
        tagAttr_case_child_correct[TagHelper.RELYON_ATTR_NAME] = '1';

        //incorrect root node case
        var tagAttr_case_root_incorrect = {};
        tagAttr_case_root_incorrect[TagHelper.NODE_NAME] = TagHelper.CLASSIFICATION_NODE_NAME;
        tagAttr_case_root_incorrect[TagHelper.TYPE_ATTR_NAME] = undefined;

        //incorrect tag node case
        var tagAttr_case_child_incorrect = {};
        tagAttr_case_child_incorrect[TagHelper.NODE_NAME] = TagHelper.TAG_NODE_NAME;
        tagAttr_case_child_incorrect[TagHelper.NAME_ATTR_NAME] = undefined;
        tagAttr_case_child_incorrect[TagHelper.VALUES_ATTR_NAME] = undefined;

        //empty node
        var tagAttr_case_empty = {};

        expect(TagHelper.checkXmlAttrs(tagAttr_case_root_correct), 'correct root node case: ').to.be.true;
        expect(TagHelper.checkXmlAttrs(tagAttr_case_child_correct), 'correct tag node case: ').to.be.true;
        expect(TagHelper.checkXmlAttrs(tagAttr_case_root_incorrect), 'incorrect root node case: ').to.be.false;
        expect(TagHelper.checkXmlAttrs(tagAttr_case_child_incorrect), 'incorrect tag node case: ').to.be.false;
        expect(TagHelper.checkXmlAttrs(tagAttr_case_empty), 'empty node: ').to.be.false;
    });

    //unit test for TagHelper.completeTagAttrs() method
    it('case: completeTagAttrs()', function(){

        //empty case
        var tagAttr_case_empty = {};

        //with values case
        var tagAttr_case_with_value = {};
        tagAttr_case_with_value[TagHelper.VALUES_ATTR_NAME] = '1|2|3';
        
        TagHelper.completeTagAttrs(tagAttr_case_empty);
        TagHelper.completeTagAttrs(tagAttr_case_with_value);

        expect(tagAttr_case_empty, 'empty case: ').to.property(TagHelper.DEFAULT_ATTR_NAME, '');
        expect(tagAttr_case_empty, 'empty case').to.property(TagHelper.EDITABLE_ATTR_NAME, 'false');
        expect(tagAttr_case_empty, 'empty case: ').to.property(TagHelper.RELYON_ATTR_NAME, '');

        expect(tagAttr_case_with_value, 'with value case: ').to.property(TagHelper.DEFAULT_ATTR_NAME, '1');
    });

    //unit test for TagHelper.convertToLowerCase() method
    it('case: convertToLowerCase()', function(){

        //correct input
        var tagAttr_case_correct = {'TYPE':'MANUAL', 'NAME':'ITAR'};

        //incorrect input
        var tagAttr_case_incorrect = {'':'ManUAL', 'NamE':null, 'VALUE':function(){}};

        TagHelper.convertToLowerCase(tagAttr_case_correct);
        TagHelper.convertToLowerCase(tagAttr_case_incorrect);

        expect(tagAttr_case_correct, 'correct case: ').to.not.property('TYPE');
        expect(tagAttr_case_correct, 'correct case: ').to.not.property('NAME');
        expect(tagAttr_case_correct, 'correct case: ').to.property('type', 'manual');
        expect(tagAttr_case_correct, 'correct case: ').to.property('name', 'itar');

        expect(tagAttr_case_incorrect, 'incorrect case: ').to.property('', 'manual');
        expect(tagAttr_case_incorrect, 'incorrect case: ').to.not.property('NamE');
        expect(tagAttr_case_incorrect, 'incorrect case: ').to.not.property('VALUE');
        expect(tagAttr_case_incorrect, 'incorrect case: ').to.property('value');
    });
});