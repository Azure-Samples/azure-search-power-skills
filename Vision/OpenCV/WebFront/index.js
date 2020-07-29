'use strict';

const images = [
    {
        name: "First",
        dataUrl: "https://raw.githubusercontent.com/bleroy/paynoattentiontothis/227cfdca6ffda032d7f8e6ad4526c6899806e9b3/img/wut.jpg",
        mimetype: "image/jpeg"
    },
    {
        name: "Second",
        dataUrl: "https://raw.githubusercontent.com/bleroy/paynoattentiontothis/b528bcda3bb21181c269cc643cc22f5ff8e08f2f/img/wut.jpg",
        mimetype: "image/jpeg"
    },
    {
        name: "Third",
        data: base64img,
        mimetype: "image/jpeg"
    }
];

$(() => {
    let currentIndex = images.length - 1;

    const maxImgSize = 600;
    const spinner = $("#spinner");
    const carousel = $("#carousel-custom");
    const imgStrip = $("#img-strip");
    const diffRightResolution = $("#diff-right-resolution");
    const diffRightImg = $("#diff-right-img");
    const swipeRightImg = $("#swipe-right-img");
    const onionRightImg = $("#onion-right-img");
    const diffLeft = $("#diff-left");
    const diffLeftResolution = $("#diff-left-resolution");
    const diffLeftImg = $("#diff-left-img");
    const swipeLeftImg = $("#swipe-left-img");
    const onionLeftImg = $("#onion-left-img");
    const sideBySideView = $("#diff-side-by-side-view");
    const swipeView = $("#diff-swipe-view");
    const onionView = $("#diff-onion-view");
    const diffView = $("#diff-diff-view");

    function imgSrc(imgData) {
        return imgData.data ? `data:${imgData.mimetype};base64,${imgData.data}` : imgData.dataUrl;
    }

    function preLoad(images, callback) {
        let i = images.length;
        images.forEach(image => {
            const img = new Image();
            img.onload = () => {
                i--;
                if (i == 0) {
                    callback();
                }
            };
            img.src = imgSrc(image);
        });
    }

    function initImageStrip(images) {
        imgStrip.empty();
        images.forEach((imgData, i) => {
            const li = $(`<li data-index="${i}"></li>`);
            if (i === 0) {
                li.addClass("active");
            }
            const img = $('<img class="align-middle">').attr("alt", imgData.name);
            img.attr("src", imgSrc(imgData));
            li.append(img);
            imgStrip.append(li);
        });
    }

    function getScale(width, height, max) {
        return max / (width > height ? width : height);
    }

    function setScale(img, scale) {
        const width = img.prop('naturalWidth');
        const height = img.prop('naturalHeight');
        img.css("width", scale * width + "px");
        img.css("height", scale * height + "px");
    }

    function selectImage(index) {
        currentIndex = index;
        imgStrip.find("li").removeClass("active");
        const selectedImg = imgStrip.find(`li[data-index='${index}'] img`);
        selectedImg.parent().addClass("active");
        const rightSrc = selectedImg.attr("src");
        diffRightImg.attr("src", rightSrc);
        swipeRightImg.attr("src", rightSrc);
        onionRightImg.attr("src", rightSrc);
        const rightWidth = selectedImg.prop('naturalWidth');
        const rightHeight = selectedImg.prop('naturalHeight');
        diffRightResolution.html(`${rightWidth} &times; ${rightHeight}`);
        const rightScale = getScale(rightWidth, rightHeight, maxImgSize);
        setScale(swipeRightImg, rightScale);
        setScale(onionRightImg, rightScale);

        if (index <= 0) {
            diffLeftImg.attr("src", "");
            swipeLeftImg.attr("src", "");
            onionLeftImg.attr("src", "");
            diffLeftResolution.empty();
            diffLeft.addClass("hidden");
            setScale(diffRightImg, rightScale);
        }
        else {
            diffLeft.removeClass("hidden");
            const previousImg = imgStrip.find(`li[data-index='${index - 1}'] img`);
            const leftSrc = previousImg.attr("src");
            diffLeftImg.attr("src", leftSrc);
            swipeLeftImg.attr("src", leftSrc);
            onionLeftImg.attr("src", leftSrc);
            const leftWidth = previousImg.prop('naturalWidth');
            const leftHeight = previousImg.prop('naturalHeight');
            diffLeftResolution.html(`${leftWidth} &times; ${leftHeight}`);
            const leftScale = getScale(leftWidth, leftHeight, maxImgSize);
            setScale(swipeLeftImg, leftScale);
            setScale(onionLeftImg, leftScale);
            const scale = Math.min(leftScale, rightScale);
            setScale(diffRightImg, scale);
            setScale(diffLeftImg, scale);
        }
    }

    preLoad(images, () => {
        spinner.addClass("hidden");

        initImageStrip(images);
        selectImage(currentIndex);

        carousel.removeClass("hidden");

        $("#img-strip li").click(e => {
            const selectedIndex = parseInt($(e.delegateTarget).data("index"), 10);
            selectImage(selectedIndex);
        });

        $(".carousel-control.left").click(e => {
            if (currentIndex > 0) {
                selectImage(currentIndex - 1);
            }
        });

        $(".carousel-control.right").click(e => {
            if (currentIndex < images.length - 1) {
                selectImage(currentIndex + 1);
            }
        });

        $("input[type=radio][name=diff-view]").change(e => {
            const selection = $(e.delegateTarget).data("selects");
            [sideBySideView, swipeView, onionView, diffView]
                .forEach(view => {
                    if (view.attr("id") == selection) {
                        view.removeClass("hidden");
                    }
                    else {
                        view.addClass("hidden");
                    }
                });
        });

        $("#diff-swipe-slider").change(e => {
            const percentage = parseFloat($(e.delegateTarget).val());
            const width = swipeRightImg.prop("offsetWidth") + 1;
            swipeRightImg.css("clip", `rect(0, ${width * percentage / 100}px, auto, auto)`);
        });

        $("#diff-onion-slider").change(e => {
            const percentage = parseFloat($(e.delegateTarget).val());
            onionRightImg.css("opacity", (percentage / 100));
        });
    });
});